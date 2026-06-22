namespace TeamFlow.DTOs.Tasks
{
    public class TaskQaAssignmentDto
    {
        public int TaskId { get; set; }
        public int QaUserId { get; set; }
        public string? QaUserName { get; set; }
        public string? QaUserEmail { get; set; }
        public int AssignedByUserId { get; set; }
        public DateTime AssignedAt { get; set; }
    }
}
