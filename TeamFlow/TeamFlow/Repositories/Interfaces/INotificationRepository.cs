using TeamFlow.Entities;

namespace TeamFlow.Repositories.Interfaces
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetByUserIdAsync(int userId, int take = 50);
        Task<int> GetUnreadCountAsync(int userId);
        Task<Notification?> GetForUserAsync(int id, int userId);
    }
}
