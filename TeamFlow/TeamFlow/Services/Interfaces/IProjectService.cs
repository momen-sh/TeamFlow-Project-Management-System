using TeamFlow.Entities;
using TeamFlow.Services.Results;

namespace TeamFlow.Services.Interfaces
{
    public interface IProjectService
    {
        Task<IEnumerable<Project>> GetAllAsync();
        Task<IEnumerable<Project>> GetAssignedToUserAsync(int userId);
        Task<Project?> GetByIdAsync(int id);
        Task<IEnumerable<Project>> GetByWorkspaceIdAsync(int workspaceId);
        Task<Project> CreateAsync(Project project);
        Task<ServiceResult<Project>> UpdateAsync(Project project);
        Task<bool> DeleteAsync(int id);
        Task<ServiceResult<int>> AssignUsersAsync(int projectId, IEnumerable<int> userIds);
        Task<ServiceResult<int>> UnassignUserAsync(int projectId, int userId);
    }
}
