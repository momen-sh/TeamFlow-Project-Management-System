using System.ComponentModel.DataAnnotations;
using TeamFlow.Authorization;

namespace TeamFlow.Entities
{
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required, MaxLength(100), EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = AppRoles.Developer;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ProjectMember> Projects { get; set; } = new List<ProjectMember>();
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
