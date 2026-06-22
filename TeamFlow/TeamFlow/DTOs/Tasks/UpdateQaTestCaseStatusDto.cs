using System.ComponentModel.DataAnnotations;

namespace TeamFlow.DTOs.Tasks
{
    public class UpdateQaTestCaseStatusDto
    {
        [Required]
        public QaTestCaseStatus Status { get; set; }
    }
}
