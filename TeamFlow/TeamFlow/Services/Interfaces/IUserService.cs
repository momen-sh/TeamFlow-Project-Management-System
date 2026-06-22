using TeamFlow.Entities;
using TeamFlow.Services.Results;

namespace TeamFlow.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<IEnumerable<User>> GetByRoleAsync(string role);
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task<ServiceResult<User>> CreateAsync(User user, string password, string? role);
        Task<ServiceResult<User>> UpdateAsync(User user, string? role);
        Task<bool> DeleteAsync(int id);
    }
}
