namespace TeamFlow.DTOs.Tasks
{
    public class TaskWorkRecordDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int TimeSpentMinutes { get; set; }
        public string BranchNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int CreatedByUserId { get; set; }
        public string? CreatedByUserName { get; set; }
        public int TaskId { get; set; }
    }
}
