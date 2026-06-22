using System.ComponentModel.DataAnnotations;

namespace TeamFlow.DTOs.Projects
{
    public class CreateProjectDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int? WorkspaceId { get; set; }
    }
}
