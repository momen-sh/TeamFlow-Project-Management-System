using Microsoft.EntityFrameworkCore;
using TeamFlow.Data;
using TeamFlow.Entities;
using TeamFlow.Repositories.Interfaces;

namespace TeamFlow.Repositories.Implementations
{
    public class ProjectRepository : GenericRepository<Project>, IProjectRepository
    {
        public ProjectRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Project>> GetByWorkspaceIdAsync(int workspaceId)
        {
            return await _context.Projects
                .AsNoTracking()
                .Include(x => x.Owner)
                .Include(x => x.Members)
                .ThenInclude(x => x.User)
                .Where(x => x.WorkspaceId == workspaceId)
                .ToListAsync();
        }

        public override async Task<IEnumerable<Project>> GetAllAsync()
        {
            return await _context.Projects
                .AsNoTracking()
                .Include(x => x.Owner)
                .Include(x => x.Members)
                .ThenInclude(x => x.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<Project>> GetAssignedToUserAsync(int userId)
        {
            return await _context.Projects
                .AsNoTracking()
                .Include(x => x.Owner)
                .Include(x => x.Members)
                .ThenInclude(x => x.User)
                .Where(x => x.Members.Any(m => m.UserId == userId))
                .ToListAsync();
        }

        public async Task<Project?> GetWithMembersAsync(int id)
        {
            return await _context.Projects
                .AsNoTracking()
                .Include(x => x.Owner)
                .Include(x => x.Members)
                .ThenInclude(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Projects.AnyAsync(x => x.Id == id);
        }

        public async Task<bool> IsUserAssignedToProjectAsync(int projectId, int userId)
        {
            return await _context.Projects.AnyAsync(x =>
                x.Id == projectId &&
                x.Members.Any(m => m.UserId == userId));
        }

        public async Task<bool> IsProjectOwnerAsync(int projectId, int userId)
        {
            return await _context.Projects.AnyAsync(x => x.Id == projectId && x.OwnerId == userId);
        }

        public async Task AssignUserAsync(int projectId, int userId)
        {
            var existing = await _context.ProjectMembers
                .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.UserId == userId);

            if (existing is null)
            {
                await _context.ProjectMembers.AddAsync(new ProjectMember
                {
                    ProjectId = projectId,
                    UserId = userId,
                    Role = "Member"
                });
                return;
            }

            existing.Role = "Member";
        }

        public async Task<int> AssignUsersAsync(int projectId, IEnumerable<int> userIds)
        {
            var distinctIds = userIds.Distinct().ToList();
            if (distinctIds.Count == 0)
            {
                return 0;
            }

            var existingMembers = await _context.ProjectMembers
                .Where(x => x.ProjectId == projectId && distinctIds.Contains(x.UserId))
                .ToDictionaryAsync(x => x.UserId, x => x);

            var newMembers = distinctIds
                .Where(id => !existingMembers.ContainsKey(id))
                .Select(id => new ProjectMember
                {
                    ProjectId = projectId,
                    UserId = id,
                    Role = "Member"
                })
                .ToList();

            if (newMembers.Count > 0)
            {
                await _context.ProjectMembers.AddRangeAsync(newMembers);
            }

            foreach (var member in existingMembers.Values)
            {
                member.Role = "Member";
            }

            return distinctIds.Count;
        }

        public async Task<int> CountAsync()
        {
            return await _context.Projects.CountAsync();
        }

        public async Task<IEnumerable<Project>> GetRecentAsync(int count)
        {
            return await _context.Projects
                .OrderByDescending(x => x.CreatedAt)
                .Take(count)
                .ToListAsync();
        }
        public async Task<int> UnassignUserAsync(int projectId, int userId)
        {
            var member = await _context.ProjectMembers
                .FirstOrDefaultAsync(x =>
                    x.ProjectId == projectId &&
                    x.UserId == userId);

            if (member is null)
                return 0;

            _context.ProjectMembers.Remove(member);
            return 1;
        }

    }
}
