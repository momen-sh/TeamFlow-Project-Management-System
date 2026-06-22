using Microsoft.EntityFrameworkCore;
using TeamFlow.Data;
using TeamFlow.Entities;
using TeamFlow.Repositories.Interfaces;

namespace TeamFlow.Repositories.Implementations
{
    public class QaTestCaseRepository : GenericRepository<QaTestCase>, IQaTestCaseRepository
    {
        public QaTestCaseRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<QaTestCase>> GetByTaskIdAsync(int taskId)
        {
            return await _context.QaTestCases
                .AsNoTracking()
                .Include(x => x.CreatedByUser)
                .Where(x => x.TaskId == taskId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }
    }
}
