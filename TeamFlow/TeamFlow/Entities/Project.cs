using System.ComponentModel.DataAnnotations;

namespace TeamFlow.Entities
{
    public class Project
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int OwnerId { get; set; }
        public User Owner { get; set; } = null!;

        public int? WorkspaceId { get; set; }
        public Workspace? Workspace { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
        public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    }
}
