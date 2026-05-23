using Microsoft.EntityFrameworkCore;
using TeamFlow.Data;
using TeamFlow.Entities;
using TeamFlow.Repositories.Interfaces;

namespace TeamFlow.Repositories.Implementations
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Notification>> GetByUserIdAsync(int userId, int take = 50)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.Notifications.CountAsync(x => x.UserId == userId && !x.IsRead);
        }

        public async Task<Notification?> GetForUserAsync(int id, int userId)
        {
            return await _context.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        }
    }
}
