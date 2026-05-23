using System.ComponentModel.DataAnnotations;

namespace TeamFlow.DTOs.Tasks
{
    public class SendTaskToQaDto
    {
        [Required, MinLength(1)]
        public IEnumerable<int> QaUserIds { get; set; } = new List<int>();
    }
}
