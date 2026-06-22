using TeamFlow.Entities;
using TeamFlow.Services.Results;

namespace TeamFlow.Services.Interfaces
{
    public interface ITaskWorkRecordService
    {
        Task<IEnumerable<TaskWorkRecord>> GetByTaskIdAsync(int taskId);
        Task<ServiceResult<TaskWorkRecord>> CreateAsync(int taskId, TaskWorkRecord record, int currentUserId, string? currentUserRole);
    }
}
