using TeamFlow.Entities;

namespace TeamFlow.Repositories.Interfaces
{
    public interface IProjectRepository : IGenericRepository<Project>
    {
        Task<IEnumerable<Project>> GetByWorkspaceIdAsync(int workspaceId);
        Task<IEnumerable<Project>> GetAssignedToUserAsync(int userId);
        Task<Project?> GetWithMembersAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> IsUserAssignedToProjectAsync(int projectId, int userId);
        Task<bool> IsProjectOwnerAsync(int projectId, int userId);
        Task AssignUserAsync(int projectId, int userId);
        Task<int> AssignUsersAsync(int projectId, IEnumerable<int> userIds);
        Task<int> CountAsync();
        Task<IEnumerable<Project>> GetRecentAsync(int count);
        Task<int> UnassignUserAsync(int projectId, int userId);
    }
}
