using Microsoft.EntityFrameworkCore;
using TeamFlow.Data;
using TeamFlow.Entities;
using TeamFlow.Repositories.Interfaces;

namespace TeamFlow.Repositories.Implementations
{
    public class CommentRepository : GenericRepository<Comment>, ICommentRepository
    {
        public CommentRepository(AppDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Comment>> GetAllAsync()
        {
            return await _context.Comments
                .Include(x => x.User)
                .Include(x => x.TaskItem)
                .Include(x => x.Mentions)
                .ToListAsync();
        }

        public override async Task<Comment?> GetByIdAsync(int id)
        {
            return await _context.Comments
                .Include(x => x.User)
                .Include(x => x.TaskItem)
                .Include(x => x.Mentions)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<Comment>> GetByTaskIdAsync(int taskId)
        {
            return await _context.Comments
                .Include(x => x.User)
                .Include(x => x.TaskItem)
                .Include(x => x.Mentions)
                .Where(x => x.TaskItemId == taskId)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();
        }
    }
}
