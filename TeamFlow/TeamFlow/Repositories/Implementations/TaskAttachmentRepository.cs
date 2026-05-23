using Microsoft.EntityFrameworkCore;
using TeamFlow.Data;
using TeamFlow.Entities;
using TeamFlow.Repositories.Interfaces;

namespace TeamFlow.Repositories.Implementations
{
    public class TaskAttachmentRepository : GenericRepository<TaskAttachment>, ITaskAttachmentRepository
    {
        public TaskAttachmentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TaskAttachment>> GetByTaskIdAsync(int taskId)
        {
            return await _context.TaskAttachments
                .Where(x => x.TaskId == taskId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }
    }
}
