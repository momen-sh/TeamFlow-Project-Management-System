using Microsoft.AspNetCore.Http;
using TeamFlow.Services.Results;

namespace TeamFlow.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<ServiceResult<string>> SaveAsync(IFormFile file, string folder, CancellationToken cancellationToken = default);
        Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default);
    }
}
