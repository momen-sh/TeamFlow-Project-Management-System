namespace TeamFlow.DTOs.Tasks
{
    public class QaTestCaseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Steps { get; set; } = string.Empty;
        public string ExpectedResult { get; set; } = string.Empty;
        public string? ActualResult { get; set; }
        public QaTestCaseStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TaskId { get; set; }
        public int CreatedByUserId { get; set; }
        public string? CreatedByUserName { get; set; }
    }
}
