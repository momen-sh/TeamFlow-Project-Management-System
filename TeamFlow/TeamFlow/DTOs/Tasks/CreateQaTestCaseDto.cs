using System.ComponentModel.DataAnnotations;

namespace TeamFlow.DTOs.Tasks
{
    public class CreateQaTestCaseDto
    {
        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string Steps { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string ExpectedResult { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? ActualResult { get; set; }

        [Required]
        public QaTestCaseStatus Status { get; set; }
    }
}
