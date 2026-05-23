using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using TeamFlow.Authorization;
using TeamFlow.DTOs.Notifications;
using TeamFlow.Entities;
using TeamFlow.Hubs;
using TeamFlow.Repositories.Interfaces;
using TeamFlow.Services.Interfaces;
using TeamFlow.Services.Results;

namespace TeamFlow.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IMapper _mapper;

        public NotificationService(
            INotificationRepository notificationRepository,
            IUserRepository userRepository,
            IHubContext<NotificationHub> hubContext,
            IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _hubContext = hubContext;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Notification>> GetByUserIdAsync(int userId)
            => await _notificationRepository.GetByUserIdAsync(userId);

        public async Task<int> GetUnreadCountAsync(int userId)
            => await _notificationRepository.GetUnreadCountAsync(userId);

        public async Task<ServiceResult<Notification>> CreateAsync(Notification notification)
        {
            if (!await _userRepository.ExistsAsync(notification.UserId))
                return ServiceResult<Notification>.Failure("User not found");

            notification.CreatedAt = DateTime.UtcNow;
            await _notificationRepository.AddAsync(notification);
            await _notificationRepository.SaveAsync();

            await _hubContext.Clients
                .Group(NotificationHub.UserGroup(notification.UserId))
                .SendAsync("notificationReceived", _mapper.Map<NotificationDto>(notification));
            await SendUnreadCountAsync(notification.UserId);

            return ServiceResult<Notification>.Success(notification);
        }

        public async Task<ServiceResult<Notification>> MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _notificationRepository.GetForUserAsync(notificationId, userId);
            if (notification is null)
                return ServiceResult<Notification>.Failure("Notification not found");

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await _notificationRepository.SaveAsync();
                await SendUnreadCountAsync(userId);
            }

            return ServiceResult<Notification>.Success(notification);
        }

        public async Task<IEnumerable<Notification>> NotifyQaTaskSentToQaAsync(TaskItem task, int senderUserId, IEnumerable<int> qaUserIds)
        {
            var receiverIds = qaUserIds.Distinct().Where(id => id != senderUserId).ToList();
            var notifications = receiverIds
                .Select(userId => new Notification
                {
                    Title = "Task sent to QA",
                    Message = $"Task \"{task.Title}\" was sent to QA.",
                    Type = NotificationType.TaskSentToQA,
                    UserId = userId,
                    RelatedEntityId = task.Id,
                    RelatedEntityType = "Task",
                    CreatedAt = DateTime.UtcNow
                })
                .ToList();

            await CreateManyAsync(notifications);
            return notifications;
        }

        public async Task<IEnumerable<Notification>> NotifyMentionedUsersAsync(Comment comment, IEnumerable<int> mentionedUserIds, int senderUserId)
        {
            var sender = await _userRepository.GetByIdAsync(senderUserId);
            var senderName = sender is null ? "A teammate" : $"{sender.FirstName} {sender.LastName}".Trim();
            var ids = mentionedUserIds.Distinct().Where(id => id != senderUserId).ToList();

            var notifications = ids.Select(userId => new Notification
            {
                Title = "You were mentioned",
                Message = $"{senderName} mentioned you in a task comment.",
                Type = NotificationType.Mention,
                UserId = userId,
                RelatedEntityId = comment.TaskItemId,
                RelatedEntityType = "Task",
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await CreateManyAsync(notifications);
            return notifications;
        }

        private async Task CreateManyAsync(IEnumerable<Notification> notifications)
        {
            var created = notifications.ToList();
            foreach (var notification in created)
            {
                await _notificationRepository.AddAsync(notification);
            }

            if (created.Count == 0)
                return;

            await _notificationRepository.SaveAsync();

            foreach (var notification in created)
            {
                await _hubContext.Clients
                    .Group(NotificationHub.UserGroup(notification.UserId))
                    .SendAsync("notificationReceived", _mapper.Map<NotificationDto>(notification));
                await SendUnreadCountAsync(notification.UserId);
            }
        }

        private async Task SendUnreadCountAsync(int userId)
        {
            var unreadCount = await _notificationRepository.GetUnreadCountAsync(userId);
            await _hubContext.Clients
                .Group(NotificationHub.UserGroup(userId))
                .SendAsync("unreadCountChanged", unreadCount);
        }
    }
}
