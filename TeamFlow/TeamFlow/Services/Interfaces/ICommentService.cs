using TeamFlow.Entities;

namespace TeamFlow.Services.Interfaces
{
    public interface ICommentService
    {
        Task<IEnumerable<Comment>> GetAllAsync();
        Task<Comment?> GetByIdAsync(int id);
        Task<IEnumerable<Comment>> GetByTaskIdAsync(int taskId);
        Task<Comment> CreateAsync(Comment comment, IEnumerable<int>? mentionedUserIds = null);
        Task<bool> DeleteAsync(int id);
    }
}
