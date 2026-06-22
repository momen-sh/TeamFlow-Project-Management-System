namespace TeamFlow.DTOs.Comments
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public int TaskItemId { get; set; }
        public int TaskId { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public IEnumerable<int> MentionedUserIds { get; set; } = new List<int>();
    }
}
