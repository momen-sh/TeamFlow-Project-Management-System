using TeamFlow.Entities;
using TeamFlow.Authorization;
using TeamFlow.Repositories.Interfaces;
using TeamFlow.Services.Interfaces;
using TeamFlow.Services.Results;

namespace TeamFlow.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
            => await _userRepository.GetAllAsync();

        public async Task<IEnumerable<User>> GetByRoleAsync(string role)
            => await _userRepository.GetByRoleAsync(role);

        public async Task<User?> GetByIdAsync(int id)
            => await _userRepository.GetByIdAsync(id);

        public async Task<User?> GetByEmailAsync(string email)
            => await _userRepository.GetByEmailAsync(email);

        public async Task<ServiceResult<User>> CreateAsync(User user, string password, string? role)
        {
            var normalizedRole = string.IsNullOrWhiteSpace(role) ? AppRoles.Developer : role;
            if (!AppRoles.All.Contains(normalizedRole))
                return ServiceResult<User>.Failure("Invalid role");

            user.Role = normalizedRole;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

            await _userRepository.AddAsync(user);
            await _userRepository.SaveAsync();
            return ServiceResult<User>.Success(user);
        }

        public async Task<ServiceResult<User>> UpdateAsync(User user, string? role)
        {
            var normalizedRole = string.IsNullOrWhiteSpace(role) ? AppRoles.Developer : role;
            if (!AppRoles.All.Contains(normalizedRole))
                return ServiceResult<User>.Failure("Invalid role");

            user.Role = normalizedRole;
            _userRepository.Update(user);
            await _userRepository.SaveAsync();
            return ServiceResult<User>.Success(user);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user is null) return false;

            _userRepository.Delete(user);
            await _userRepository.SaveAsync();
            return true;
        }
    }
}
