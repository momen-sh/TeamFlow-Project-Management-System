using Microsoft.EntityFrameworkCore;
using TeamFlow.Data;
using TeamFlow.Entities;
using TeamFlow.Repositories.Interfaces;

namespace TeamFlow.Repositories.Implementations
{
    public class TaskRepository : GenericRepository<TaskItem>, ITaskRepository
    {
        public TaskRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TaskItem>> GetByProjectIdAsync(int projectId)
        {
            return await _context.Tasks
                .Include(x => x.Project)
                .ThenInclude(x => x.Members)
                .Include(x => x.AssignedUser)
                .Where(x => x.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetAssignedToUserAsync(int userId)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Include(x => x.Project)
                .ThenInclude(x => x.Members)
                .Include(x => x.AssignedUser)
                .Include(x => x.Attachments)
                .Include(x => x.WorkRecords)
                .ThenInclude(x => x.CreatedByUser)
                .Include(x => x.QaTestCases)
                .ThenInclude(x => x.CreatedByUser)
                .Include(x => x.QaAssignments)
                .ThenInclude(x => x.QaUser)
                .Where(x => x.AssignedUserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetVisibleToUserAsync(int userId)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Include(x => x.Project)
                .ThenInclude(x => x.Members)
                .Include(x => x.AssignedUser)
                .Include(x => x.Attachments)
                .Include(x => x.WorkRecords)
                .ThenInclude(x => x.CreatedByUser)
                .Include(x => x.QaTestCases)
                .ThenInclude(x => x.CreatedByUser)
                .Include(x => x.QaAssignments)
                .ThenInclude(x => x.QaUser)
                .Where(x =>
                    x.AssignedUserId == userId ||
                    x.QaAssignments.Any(qa => qa.QaUserId == userId) ||
                    x.Project.Members.Any(member => member.UserId == userId))
                .ToListAsync();
        }

        public override async Task<IEnumerable<TaskItem>> GetAllAsync()
        {
            return await _context.Tasks
                .AsNoTracking()
                .Include(x => x.Project)
                .ThenInclude(x => x.Members)
                .Include(x => x.AssignedUser)
                .Include(x => x.Attachments)
                .Include(x => x.WorkRecords)
                .ThenInclude(x => x.CreatedByUser)
                .Include(x => x.QaTestCases)
                .ThenInclude(x => x.CreatedByUser)
                .Include(x => x.QaAssignments)
                .ThenInclude(x => x.QaUser)
                .ToListAsync();
        }

        public override async Task<TaskItem?> GetByIdAsync(int id)
        {
            return await GetWithProjectMembersAsync(id);
        }

        public async Task<TaskItem?> GetWithProjectMembersAsync(int id)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Include(x => x.Project)
                .ThenInclude(x => x.Members)
                .Include(x => x.AssignedUser)
                .Include(x => x.Attachments)
                .Include(x => x.WorkRecords)
                .ThenInclude(x => x.CreatedByUser)
                .Include(x => x.QaTestCases)
                .ThenInclude(x => x.CreatedByUser)
                .Include(x => x.QaAssignments)
                .ThenInclude(x => x.QaUser)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<int> CountAsync()
        {
            return await _context.Tasks.CountAsync();
        }

        public async Task<IDictionary<int, int>> CountByStatusAsync()
        {
            return await _context.Tasks
                .GroupBy(x => x.Status)
                .Select(x => new { Status = (int)x.Key, Count = x.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);
        }

        public async Task<IDictionary<int, int>> CountByTypeAsync()
        {
            return await _context.Tasks
                .GroupBy(x => x.Type)
                .Select(x => new { Type = (int)x.Key, Count = x.Count() })
                .ToDictionaryAsync(x => x.Type, x => x.Count);
        }

        public async Task<IEnumerable<TaskItem>> GetRecentAsync(int count)
        {
            return await _context.Tasks
                .OrderByDescending(x => x.CreatedAt)
                .Take(count)
                .ToListAsync();
        }
        public async Task<TaskItem?> FindAsync(int id)
        {
            return await _context.Tasks.FindAsync(id);
        }

        public async Task<TaskItem?> GetTrackedWithQaAssignmentsAsync(int id)
        {
            return await _context.Tasks
                .Include(x => x.Project)
                .Include(x => x.QaAssignments)
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
