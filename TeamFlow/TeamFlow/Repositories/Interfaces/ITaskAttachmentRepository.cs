using TeamFlow.Entities;

namespace TeamFlow.Repositories.Interfaces
{
    public interface ITaskAttachmentRepository : IGenericRepository<TaskAttachment>
    {
        Task<IEnumerable<TaskAttachment>> GetByTaskIdAsync(int taskId);
    }
}
