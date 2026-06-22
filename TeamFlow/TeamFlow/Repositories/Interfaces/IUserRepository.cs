using TeamFlow.Entities;

namespace TeamFlow.Repositories.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<bool> ExistsAsync(int id);
        Task<HashSet<int>> GetExistingUserIdsAsync(IEnumerable<int> userIds);
        Task<IEnumerable<User>> GetByRoleAsync(string role);
        Task<IEnumerable<User>> SearchMentionTargetsAsync(IEnumerable<string> mentionTokens);
    }
}
