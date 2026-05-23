using TeamFlow.Authorization;
using TeamFlow.Entities;
using TeamFlow.Repositories.Interfaces;
using TeamFlow.Services.Interfaces;
using TeamFlow.Services.Results;

namespace TeamFlow.Services.Implementations
{
    public class TaskWorkRecordService : ITaskWorkRecordService
    {
        private readonly ITaskWorkRecordRepository _workRecordRepository;
        private readonly ITaskRepository _taskRepository;

        public TaskWorkRecordService(ITaskWorkRecordRepository workRecordRepository, ITaskRepository taskRepository)
        {
            _workRecordRepository = workRecordRepository;
            _taskRepository = taskRepository;
        }

        public async Task<IEnumerable<TaskWorkRecord>> GetByTaskIdAsync(int taskId)
            => await _workRecordRepository.GetByTaskIdAsync(taskId);

        public async Task<ServiceResult<TaskWorkRecord>> CreateAsync(int taskId, TaskWorkRecord record, int currentUserId, string? currentUserRole)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task is null)
                return ServiceResult<TaskWorkRecord>.Failure("Task not found");

            if (!CanCreate(task, currentUserId, currentUserRole))
                return ServiceResult<TaskWorkRecord>.Failure("Only the assigned user, Admin, or TeamLeader can add work records");

            record.TaskId = taskId;
            record.CreatedByUserId = currentUserId;
            record.CreatedAt = DateTime.UtcNow;

            await _workRecordRepository.AddAsync(record);
            await _workRecordRepository.SaveAsync();
            return ServiceResult<TaskWorkRecord>.Success(record);
        }

        private static bool CanCreate(TaskItem task, int userId, string? role)
            => role is AppRoles.Admin or AppRoles.TeamLeader || task.AssignedUserId == userId;
    }
}
