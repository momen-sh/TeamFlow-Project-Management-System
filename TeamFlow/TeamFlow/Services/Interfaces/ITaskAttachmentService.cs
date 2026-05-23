using Microsoft.AspNetCore.Http;
using TeamFlow.Entities;
using TeamFlow.Services.Results;

namespace TeamFlow.Services.Interfaces
{
    public interface ITaskAttachmentService
    {
        Task<IEnumerable<TaskAttachment>> GetByTaskIdAsync(int taskId);
        Task<ServiceResult<TaskAttachment>> UploadAsync(int taskId, IFormFile file, CancellationToken cancellationToken = default);
        Task<ServiceResult<object>> DeleteAsync(int taskId, int attachmentId, CancellationToken cancellationToken = default);
    }
}
