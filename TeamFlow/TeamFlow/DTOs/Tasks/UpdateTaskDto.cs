using System.ComponentModel.DataAnnotations;

namespace TeamFlow.DTOs.Tasks
{
    public class UpdateTaskDto
    {
        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public TaskType Type { get; set; }

        [Required]
        public int ProjectId { get; set; }

        public int? AssignedUserId { get; set; }
    }
}
