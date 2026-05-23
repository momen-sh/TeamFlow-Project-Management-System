namespace TeamFlow.DTOs.Tasks
{
    public class TaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public TaskType Type { get; set; }
        public int ProjectId { get; set; }
        public int? AssignedUserId { get; set; }
        public string? ProjectName { get; set; }
        public string? AssignedUserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SentToQaAt { get; set; }
        public int? SentToQaByUserId { get; set; }
        public TaskPermissionsDto Permissions { get; set; } = new();
        public IEnumerable<TaskAttachmentDto> Attachments { get; set; } = new List<TaskAttachmentDto>();
        public IEnumerable<TaskWorkRecordDto> WorkRecords { get; set; } = new List<TaskWorkRecordDto>();
        public IEnumerable<QaTestCaseDto> QaTestCases { get; set; } = new List<QaTestCaseDto>();
        public IEnumerable<TaskQaAssignmentDto> QaAssignments { get; set; } = new List<TaskQaAssignmentDto>();
    }
}
