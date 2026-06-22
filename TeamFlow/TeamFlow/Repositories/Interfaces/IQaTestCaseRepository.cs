using TeamFlow.Entities;

namespace TeamFlow.Repositories.Interfaces
{
    public interface IQaTestCaseRepository : IGenericRepository<QaTestCase>
    {
        Task<IEnumerable<QaTestCase>> GetByTaskIdAsync(int taskId);
    }
}
