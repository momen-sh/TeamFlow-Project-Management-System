using AutoMapper;
using TeamFlow.DTOs.Dashboard;
using TeamFlow.DTOs.Projects;
using TeamFlow.DTOs.Tasks;
using TeamFlow.Repositories.Interfaces;
using TeamFlow.Services.Interfaces;

namespace TeamFlow.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IMapper _mapper;

        public DashboardService(
            IProjectRepository projectRepository,
            ITaskRepository taskRepository,
            IMapper mapper)
        {
            _projectRepository = projectRepository;
            _taskRepository = taskRepository;
            _mapper = mapper;
        }

        public async Task<DashboardDto> GetAsync()
        {
            var recentTasks = await _taskRepository.GetRecentAsync(10);
            var recentProjects = await _projectRepository.GetRecentAsync(10);

            return new DashboardDto
            {
                TotalProjects = await _projectRepository.CountAsync(),
                TotalTasks = await _taskRepository.CountAsync(),
                TasksByStatus = await _taskRepository.CountByStatusAsync(),
                TasksByType = await _taskRepository.CountByTypeAsync(),
                RecentTasks = _mapper.Map<IEnumerable<TaskDto>>(recentTasks),
                RecentProjects = _mapper.Map<IEnumerable<ProjectDto>>(recentProjects)
            };
        }
    }
}
