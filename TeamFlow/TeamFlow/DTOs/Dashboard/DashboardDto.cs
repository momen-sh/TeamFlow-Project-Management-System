using TeamFlow.DTOs.Projects;
using TeamFlow.DTOs.Tasks;

namespace TeamFlow.DTOs.Dashboard
{
    public class DashboardDto
    {
        public int TotalProjects { get; set; }
        public int TotalTasks { get; set; }
        public IDictionary<int, int> TasksByStatus { get; set; } = new Dictionary<int, int>();
        public IDictionary<int, int> TasksByType { get; set; } = new Dictionary<int, int>();
        public IEnumerable<TaskDto> RecentTasks { get; set; } = new List<TaskDto>();
        public IEnumerable<ProjectDto> RecentProjects { get; set; } = new List<ProjectDto>();
    }
}
