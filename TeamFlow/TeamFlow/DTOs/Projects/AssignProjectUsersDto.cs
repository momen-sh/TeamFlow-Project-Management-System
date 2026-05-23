using System.ComponentModel.DataAnnotations;

namespace TeamFlow.DTOs.Projects
{
    public class AssignProjectUsersDto
    {
        [Required]
        [MinLength(1)]
        public List<int> UserIds { get; set; } = new();
    }
}
