using System.ComponentModel.DataAnnotations;

namespace TeamFlow.Entities
{
    public class TaskWorkRecord
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Range(1, int.MaxValue)]
        public int TimeSpentMinutes { get; set; }

        [Required, MaxLength(100)]
        public string BranchNumber { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; } = null!;

        public int TaskId { get; set; }
        public TaskItem Task { get; set; } = null!;
    }
}
