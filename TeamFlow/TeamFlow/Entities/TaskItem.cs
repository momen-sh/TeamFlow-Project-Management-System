using System.ComponentModel.DataAnnotations;

namespace TeamFlow.Entities
{
    public class TaskItem
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public TaskStatus Status { get; set; } = TaskStatus.ToDo;

        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        public TaskType Type { get; set; } = TaskType.Task;

        public DateTime? DueDate { get; set; }

        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public int? AssignedUserId { get; set; }
        public User? AssignedUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SentToQaAt { get; set; }
        public int? SentToQaByUserId { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<TaskAttachment> Attachments { get; set; } = new List<TaskAttachment>();
        public ICollection<TaskWorkRecord> WorkRecords { get; set; } = new List<TaskWorkRecord>();
        public ICollection<QaTestCase> QaTestCases { get; set; } = new List<QaTestCase>();
        public ICollection<TaskQaAssignment> QaAssignments { get; set; } = new List<TaskQaAssignment>();
    }
}
