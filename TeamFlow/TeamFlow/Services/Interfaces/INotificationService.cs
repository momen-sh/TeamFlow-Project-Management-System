using TeamFlow.Entities;
using TeamFlow.Services.Results;

namespace TeamFlow.Services.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<Notification>> GetByUserIdAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
        Task<ServiceResult<Notification>> CreateAsync(Notification notification);
        Task<ServiceResult<Notification>> MarkAsReadAsync(int notificationId, int userId);
        Task<IEnumerable<Notification>> NotifyQaTaskSentToQaAsync(TaskItem task, int senderUserId, IEnumerable<int> qaUserIds);
        Task<IEnumerable<Notification>> NotifyMentionedUsersAsync(Comment comment, IEnumerable<int> mentionedUserIds, int senderUserId);
    }
}
