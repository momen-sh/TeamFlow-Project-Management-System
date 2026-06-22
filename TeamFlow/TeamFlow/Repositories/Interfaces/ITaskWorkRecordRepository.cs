using TeamFlow.Entities;

namespace TeamFlow.Repositories.Interfaces
{
    public interface ITaskWorkRecordRepository : IGenericRepository<TaskWorkRecord>
    {
        Task<IEnumerable<TaskWorkRecord>> GetByTaskIdAsync(int taskId);
    }
}
