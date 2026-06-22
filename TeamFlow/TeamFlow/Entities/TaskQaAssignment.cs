namespace TeamFlow.Entities
{
    public class TaskQaAssignment
    {
        public int TaskId { get; set; }
        public TaskItem Task { get; set; } = null!;

        public int QaUserId { get; set; }
        public User QaUser { get; set; } = null!;

        public int AssignedByUserId { get; set; }
        public User AssignedByUser { get; set; } = null!;

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}
