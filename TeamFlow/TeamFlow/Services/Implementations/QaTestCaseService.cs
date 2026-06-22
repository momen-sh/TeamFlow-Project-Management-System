using TeamFlow.Authorization;
using TeamFlow.Entities;
using TeamFlow.Repositories.Interfaces;
using TeamFlow.Services.Interfaces;
using TeamFlow.Services.Results;

namespace TeamFlow.Services.Implementations
{
    public class QaTestCaseService : IQaTestCaseService
    {
        private readonly IQaTestCaseRepository _qaTestCaseRepository;
        private readonly ITaskRepository _taskRepository;

        public QaTestCaseService(IQaTestCaseRepository qaTestCaseRepository, ITaskRepository taskRepository)
        {
            _qaTestCaseRepository = qaTestCaseRepository;
            _taskRepository = taskRepository;
        }

        public async Task<IEnumerable<QaTestCase>> GetByTaskIdAsync(int taskId)
            => await _qaTestCaseRepository.GetByTaskIdAsync(taskId);

        public async Task<ServiceResult<QaTestCase>> CreateAsync(int taskId, QaTestCase testCase, int currentUserId, string? currentUserRole)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task is null)
                return ServiceResult<QaTestCase>.Failure("Task not found");

            if (!CanAccessQaSection(task, currentUserId))
                return ServiceResult<QaTestCase>.Failure("Task must be sent to QA before test cases can be added");

            if (!CanModify(task, currentUserId, currentUserRole))
                return ServiceResult<QaTestCase>.Failure("Only assigned QA, Admin, or TeamLeader users can add QA test cases");

            testCase.TaskId = taskId;
            testCase.CreatedByUserId = currentUserId;
            testCase.CreatedAt = DateTime.UtcNow;

            await _qaTestCaseRepository.AddAsync(testCase);
            await _qaTestCaseRepository.SaveAsync();
            return ServiceResult<QaTestCase>.Success(testCase);
        }

        public async Task<ServiceResult<QaTestCase>> UpdateStatusAsync(int taskId, int testCaseId, QaTestCaseStatus status, int currentUserId, string? currentUserRole)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task is null)
                return ServiceResult<QaTestCase>.Failure("Task not found");

            if (!CanModify(task, currentUserId, currentUserRole))
                return ServiceResult<QaTestCase>.Failure("Only assigned QA, Admin, or TeamLeader users can update QA test cases");

            var testCase = await _qaTestCaseRepository.GetByIdAsync(testCaseId);
            if (testCase is null || testCase.TaskId != taskId)
                return ServiceResult<QaTestCase>.Failure("QA test case not found");

            testCase.Status = status;
            _qaTestCaseRepository.Update(testCase);
            await _qaTestCaseRepository.SaveAsync();
            return ServiceResult<QaTestCase>.Success(testCase);
        }

        private static bool CanAccessQaSection(TaskItem task, int userId)
            => task.SentToQaAt.HasValue || task.QaAssignments.Any(assignment => assignment.QaUserId == userId);

        private static bool CanModify(TaskItem task, int userId, string? role)
            => role is AppRoles.Admin or AppRoles.TeamLeader ||
               (role == AppRoles.QA && task.QaAssignments.Any(assignment => assignment.QaUserId == userId));
    }
}
