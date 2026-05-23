using TeamFlow.Entities;
using TeamFlow.Services.Results;

namespace TeamFlow.Services.Interfaces
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskItem>> GetAllAsync();
        Task<IEnumerable<TaskItem>> GetAssignedToUserAsync(int userId);
        Task<IEnumerable<TaskItem>> GetVisibleToUserAsync(int userId);
        Task<TaskItem?> GetByIdAsync(int id);
        Task<IEnumerable<TaskItem>> GetByProjectIdAsync(int projectId);
        Task<ServiceResult<TaskItem>> CreateAsync(TaskItem task);
        Task<ServiceResult<TaskItem>> UpdateAsync(TaskItem task);
        Task<bool> UpdateStatusAsync(TaskItem task, TaskStatus status);
        Task<ServiceResult<TaskItem>> SendToQaAsync(int taskId, IEnumerable<int> qaUserIds, int currentUserId, string? currentUserRole);
        Task<ServiceResult<TaskItem>> SelfAssignAsync(int taskId, int currentUserId);
        Task<ServiceResult<TaskItem>> UnassignAsync(TaskItem task);
        Task<bool> DeleteAsync(int id);
    }
}
