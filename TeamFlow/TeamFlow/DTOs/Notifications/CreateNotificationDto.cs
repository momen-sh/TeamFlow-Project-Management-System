using System.ComponentModel.DataAnnotations;

namespace TeamFlow.DTOs.Notifications
{
    public class CreateNotificationDto
    {
        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        public NotificationType Type { get; set; } = NotificationType.Info;

        [Required]
        public int UserId { get; set; }

        public int? RelatedEntityId { get; set; }

        [MaxLength(50)]
        public string? RelatedEntityType { get; set; }
    }
}
