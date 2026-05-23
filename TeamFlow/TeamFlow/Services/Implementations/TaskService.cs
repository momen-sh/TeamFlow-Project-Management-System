using TeamFlow.Entities;
using TeamFlow.Authorization;
using TeamFlow.Repositories.Interfaces;
using TeamFlow.Services.Interfaces;
using TeamFlow.Services.Results;

namespace TeamFlow.Services.Implementations
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;

        public TaskService(
            ITaskRepository taskRepository,
            IProjectRepository projectRepository,
            IUserRepository userRepository,
            INotificationService notificationService)
        {
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<TaskItem>> GetAllAsync()
            => await _taskRepository.GetAllAsync();

        public async Task<IEnumerable<TaskItem>> GetAssignedToUserAsync(int userId)
            => await _taskRepository.GetAssignedToUserAsync(userId);

        public async Task<IEnumerable<TaskItem>> GetVisibleToUserAsync(int userId)
            => await _taskRepository.GetVisibleToUserAsync(userId);

        public async Task<TaskItem?> GetByIdAsync(int id)
            => await _taskRepository.GetWithProjectMembersAsync(id);

        public async Task<IEnumerable<TaskItem>> GetByProjectIdAsync(int projectId)
            => await _taskRepository.GetByProjectIdAsync(projectId);

        public async Task<ServiceResult<TaskItem>> CreateAsync(TaskItem task)
        {
            if (!await _projectRepository.ExistsAsync(task.ProjectId))
                return ServiceResult<TaskItem>.Failure("Project not found");

            if (task.AssignedUserId.HasValue)
            {
                if (!await _userRepository.ExistsAsync(task.AssignedUserId.Value))
                    return ServiceResult<TaskItem>.Failure("Assigned user not found");

                if (!await _projectRepository.IsUserAssignedToProjectAsync(task.ProjectId, task.AssignedUserId.Value))
                    return ServiceResult<TaskItem>.Failure("Assigned user does not belong to the project");
            }

            await _taskRepository.AddAsync(task);
            await _taskRepository.SaveAsync();
            return ServiceResult<TaskItem>.Success(task);
        }

        public async Task<ServiceResult<TaskItem>> UpdateAsync(TaskItem task)
        {
            if (!await _projectRepository.ExistsAsync(task.ProjectId))
                return ServiceResult<TaskItem>.Failure("Project not found");

            if (task.AssignedUserId.HasValue)
            {
                if (!await _userRepository.ExistsAsync(task.AssignedUserId.Value))
                    return ServiceResult<TaskItem>.Failure("Assigned user not found");

                if (!await _projectRepository.IsUserAssignedToProjectAsync(task.ProjectId, task.AssignedUserId.Value))
                    return ServiceResult<TaskItem>.Failure("Assigned user does not belong to the project");
            }

            _taskRepository.Update(task);
            await _taskRepository.SaveAsync();
            return ServiceResult<TaskItem>.Success(task);
        }

        public async Task<bool> UpdateStatusAsync(TaskItem task, TaskStatus status)
        {
            task.Status = status;
            _taskRepository.Update(task);
            return await _taskRepository.SaveAsync();
        }

        public async Task<ServiceResult<TaskItem>> SendToQaAsync(int taskId, IEnumerable<int> qaUserIds, int currentUserId, string? currentUserRole)
        {
            var selectedQaUserIds = qaUserIds.Distinct().ToList();
            if (selectedQaUserIds.Count == 0)
                return ServiceResult<TaskItem>.Failure("Select at least one QA user");

            var task = await _taskRepository.GetTrackedWithQaAssignmentsAsync(taskId);
            if (task is null)
                return ServiceResult<TaskItem>.Failure("Task not found");

            if (!await CanSendToQaAsync(task, currentUserId, currentUserRole))
                return ServiceResult<TaskItem>.Failure("Only the project owner, Admin, or TeamLeader can send this task to QA");

            var qaUsers = (await _userRepository.GetByRoleAsync(AppRoles.QA)).ToList();
            var validQaUserIds = qaUsers.Select(user => user.Id).ToHashSet();
            if (selectedQaUserIds.Any(id => !validQaUserIds.Contains(id)))
                return ServiceResult<TaskItem>.Failure("One or more selected users are not QA users");

            var shouldNotify = !task.SentToQaAt.HasValue;
            if (!task.SentToQaAt.HasValue)
            {
                task.SentToQaAt = DateTime.UtcNow;
                task.SentToQaByUserId = currentUserId;
            }

            var existingQaIds = task.QaAssignments.Select(x => x.QaUserId).ToHashSet();
            var newQaIds = selectedQaUserIds.Where(id => !existingQaIds.Contains(id)).ToList();
            foreach (var qaUserId in newQaIds)
            {
                task.QaAssignments.Add(new TaskQaAssignment
                {
                    TaskId = task.Id,
                    QaUserId = qaUserId,
                    AssignedByUserId = currentUserId,
                    AssignedAt = DateTime.UtcNow
                });
            }

            await _taskRepository.SaveAsync();

            var updated = await _taskRepository.GetByIdAsync(taskId);
            if (updated is not null && (shouldNotify || newQaIds.Count > 0))
                await _notificationService.NotifyQaTaskSentToQaAsync(updated, currentUserId, selectedQaUserIds);

            return updated is null
                ? ServiceResult<TaskItem>.Failure("Task not found")
                : ServiceResult<TaskItem>.Success(updated);
        }

        public async Task<ServiceResult<TaskItem>> SelfAssignAsync(int taskId, int currentUserId)
        {
            var task = await _taskRepository.GetWithProjectMembersAsync(taskId);
            if (task is null)
                return ServiceResult<TaskItem>.Failure("Task not found");

            if (task.AssignedUserId.HasValue)
                return ServiceResult<TaskItem>.Failure("Task is already assigned");

            if (!await _projectRepository.IsUserAssignedToProjectAsync(task.ProjectId, currentUserId))
                return ServiceResult<TaskItem>.Failure("You must belong to the project to self-assign this task");

            task.AssignedUserId = currentUserId;
            _taskRepository.Update(task);
            await _taskRepository.SaveAsync();

            return ServiceResult<TaskItem>.Success(task);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _taskRepository.GetByIdAsync(id);
            if (entity is null) return false;

            _taskRepository.Delete(entity);
            await _taskRepository.SaveAsync();
            return true;
        }

        public async Task<ServiceResult<TaskItem>> UnassignAsync(TaskItem task)
        {
            if (task is null)
                return ServiceResult<TaskItem>.Failure("Task not found");

            if (!task.AssignedUserId.HasValue)
                return ServiceResult<TaskItem>.Failure("Task is already unassigned");

            var entity = await _taskRepository.FindAsync(task.Id);

            if (entity is null)
                return ServiceResult<TaskItem>.Failure("Task not found");

            entity.AssignedUserId = null;

            await _taskRepository.SaveAsync();

            return ServiceResult<TaskItem>.Success(entity);
        }

        private async Task<bool> CanSendToQaAsync(TaskItem task, int userId, string? role)
        {
            if (role == AppRoles.Admin || role == AppRoles.TeamLeader)
                return true;

            if (await _projectRepository.IsProjectOwnerAsync(task.ProjectId, userId))
                return true;

            return task.AssignedUserId == userId;
        }

    }
}
