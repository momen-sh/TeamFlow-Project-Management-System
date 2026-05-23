using TeamFlow.Entities;

namespace TeamFlow.Repositories.Interfaces
{
    public interface ICommentRepository : IGenericRepository<Comment>
    {
        Task<IEnumerable<Comment>> GetByTaskIdAsync(int taskId);
    }
}