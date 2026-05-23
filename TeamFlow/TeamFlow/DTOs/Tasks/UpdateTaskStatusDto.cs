using System.ComponentModel.DataAnnotations;

namespace TeamFlow.DTOs.Tasks
{
    public class UpdateTaskStatusDto
    {
        [Required]
        public TaskStatus Status { get; set; }
    }
}
