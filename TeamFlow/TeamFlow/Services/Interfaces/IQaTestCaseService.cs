using TeamFlow.Entities;
using TeamFlow.Services.Results;

namespace TeamFlow.Services.Interfaces
{
    public interface IQaTestCaseService
    {
        Task<IEnumerable<QaTestCase>> GetByTaskIdAsync(int taskId);
        Task<ServiceResult<QaTestCase>> CreateAsync(int taskId, QaTestCase testCase, int currentUserId, string? currentUserRole);
        Task<ServiceResult<QaTestCase>> UpdateStatusAsync(int taskId, int testCaseId, QaTestCaseStatus status, int currentUserId, string? currentUserRole);
    }
}
