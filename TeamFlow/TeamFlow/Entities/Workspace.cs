using System.ComponentModel.DataAnnotations;

namespace TeamFlow.Entities
{
    public class Workspace
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int OwnerId { get; set; }
        public User Owner { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Project> Projects { get; set; } = new List<Project>();
        public ICollection<WorkspaceMember> Members { get; set; } = new List<WorkspaceMember>();
    }
}
