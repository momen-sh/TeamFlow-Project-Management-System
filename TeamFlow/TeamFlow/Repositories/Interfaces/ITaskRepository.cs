using TeamFlow.Entities;

namespace TeamFlow.Repositories.Interfaces
{
    public interface ITaskRepository : IGenericRepository<TaskItem>
    {
        Task<IEnumerable<TaskItem>> GetByProjectIdAsync(int projectId);
        Task<IEnumerable<TaskItem>> GetAssignedToUserAsync(int userId);
        Task<IEnumerable<TaskItem>> GetVisibleToUserAsync(int userId);
        Task<TaskItem?> GetWithProjectMembersAsync(int id);
        Task<int> CountAsync();
        Task<IDictionary<int, int>> CountByStatusAsync();
        Task<IDictionary<int, int>> CountByTypeAsync();
        Task<IEnumerable<TaskItem>> GetRecentAsync(int count);
        Task<TaskItem?> FindAsync(int id);
        Task<TaskItem?> GetTrackedWithQaAssignmentsAsync(int id);
    }
}
