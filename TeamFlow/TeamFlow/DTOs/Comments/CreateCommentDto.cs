using System.ComponentModel.DataAnnotations;

namespace TeamFlow.DTOs.Comments
{
    public class CreateCommentDto
    {
        [Required, MaxLength(1000)]
        public string Content { get; set; } = string.Empty;

        [Required]
        public int TaskItemId { get; set; }

        [Required]
        public int UserId { get; set; }

        public IEnumerable<int> MentionedUserIds { get; set; } = new List<int>();
    }
}
