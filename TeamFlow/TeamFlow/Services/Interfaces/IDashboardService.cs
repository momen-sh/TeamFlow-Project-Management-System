using TeamFlow.DTOs.Dashboard;

namespace TeamFlow.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetAsync();
    }
}
