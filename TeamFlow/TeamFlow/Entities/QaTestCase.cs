using System.ComponentModel.DataAnnotations;

namespace TeamFlow.Entities
{
    public class QaTestCase
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string Steps { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string ExpectedResult { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? ActualResult { get; set; }

        public QaTestCaseStatus Status { get; set; } = QaTestCaseStatus.Blocked;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; } = null!;

        public int TaskId { get; set; }
        public TaskItem Task { get; set; } = null!;
    }
}
