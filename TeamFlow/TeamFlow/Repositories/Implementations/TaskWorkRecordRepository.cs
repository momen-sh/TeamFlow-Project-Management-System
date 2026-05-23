using Microsoft.EntityFrameworkCore;
using TeamFlow.Data;
using TeamFlow.Entities;
using TeamFlow.Repositories.Interfaces;

namespace TeamFlow.Repositories.Implementations
{
    public class TaskWorkRecordRepository : GenericRepository<TaskWorkRecord>, ITaskWorkRecordRepository
    {
        public TaskWorkRecordRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TaskWorkRecord>> GetByTaskIdAsync(int taskId)
        {
            return await _context.TaskWorkRecords
                .AsNoTracking()
                .Include(x => x.CreatedByUser)
                .Where(x => x.TaskId == taskId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }
    }
}
