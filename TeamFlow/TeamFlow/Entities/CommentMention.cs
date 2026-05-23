namespace TeamFlow.Entities
{
    public class CommentMention
    {
        public int CommentId { get; set; }
        public Comment Comment { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
