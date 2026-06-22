using Microsoft.EntityFrameworkCore;
using TeamFlow.Data;
using TeamFlow.Entities;
using TeamFlow.Repositories.Interfaces;

namespace TeamFlow.Repositories.Implementations
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Users.AnyAsync(x => x.Id == id);
        }

        public async Task<HashSet<int>> GetExistingUserIdsAsync(IEnumerable<int> userIds)
        {
            var ids = userIds.Distinct().ToList();

            var result = await _context.Users
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync();

            return result.ToHashSet();
        }

        public async Task<IEnumerable<User>> GetByRoleAsync(string role)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(x => x.Role == role)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> SearchMentionTargetsAsync(IEnumerable<string> mentionTokens)
        {
            var tokens = mentionTokens
                .Select(NormalizeMentionToken)
                .Where(token => !string.IsNullOrWhiteSpace(token))
                .Distinct()
                .ToList();

            if (tokens.Count == 0)
                return new List<User>();

            var users = await _context.Users.AsNoTracking().ToListAsync();
            return users.Where(user =>
            {
                var email = user.Email.ToLowerInvariant();
                var localPart = email.Split('@')[0];
                var fullName = $"{user.FirstName}{user.LastName}".ToLowerInvariant();
                var spacedFullName = $"{user.FirstName} {user.LastName}".Trim().ToLowerInvariant();

                return tokens.Any(token =>
                    token == email ||
                    token == localPart ||
                    token == fullName ||
                    token == spacedFullName);
            }).ToList();
        }

        private static string NormalizeMentionToken(string token)
        {
            return token
                .Trim()
                .TrimStart('@')
                .TrimEnd('.', ',', ';', ':', '!', '?', ')', ']', '}')
                .ToLowerInvariant();
        }
    }
}
