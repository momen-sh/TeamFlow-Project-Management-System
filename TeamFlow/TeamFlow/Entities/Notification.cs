using System.ComponentModel.DataAnnotations;

namespace TeamFlow.Entities
{
    public class Notification
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        public NotificationType Type { get; set; } = NotificationType.Info;

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int? RelatedEntityId { get; set; }

        [MaxLength(50)]
        public string? RelatedEntityType { get; set; }
    }
}
