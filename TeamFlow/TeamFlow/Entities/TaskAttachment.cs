using System.ComponentModel.DataAnnotations;

namespace TeamFlow.Entities
{
    public class TaskAttachment
    {
        public int Id { get; set; }

        public int TaskId { get; set; }
        public TaskItem Task { get; set; } = null!;

        [Required, MaxLength(500)]
        public string FileUrl { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string FileType { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
