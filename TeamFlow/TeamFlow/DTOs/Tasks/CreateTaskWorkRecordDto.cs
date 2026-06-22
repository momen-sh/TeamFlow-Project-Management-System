using System.ComponentModel.DataAnnotations;

namespace TeamFlow.DTOs.Tasks
{
    public class CreateTaskWorkRecordDto
    {
        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Range(1, int.MaxValue)]
        public int TimeSpentMinutes { get; set; }

        [Required, MaxLength(100)]
        public string BranchNumber { get; set; } = string.Empty;
    }
}
