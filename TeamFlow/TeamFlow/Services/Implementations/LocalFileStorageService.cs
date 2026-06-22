using TeamFlow.Services.Interfaces;
using TeamFlow.Services.Results;

namespace TeamFlow.Services.Implementations
{
    public class LocalFileStorageService : IFileStorageService
    {
        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/webp",
            "video/mp4",
            "video/webm",
            "video/quicktime"
        };

        private readonly IWebHostEnvironment _environment;

        public LocalFileStorageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<ServiceResult<string>> SaveAsync(IFormFile file, string folder, CancellationToken cancellationToken = default)
        {
            if (file.Length == 0)
                return ServiceResult<string>.Failure("File is empty");

            if (!AllowedContentTypes.Contains(file.ContentType))
                return ServiceResult<string>.Failure("Only image and video files are allowed");

            var uploadsRoot = Path.Combine(_environment.ContentRootPath, "Uploads", folder);
            Directory.CreateDirectory(uploadsRoot);

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(uploadsRoot, fileName);

            await using var stream = File.Create(fullPath);
            await file.CopyToAsync(stream, cancellationToken);

            var url = $"/uploads/{folder}/{fileName}".Replace("\\", "/");
            return ServiceResult<string>.Success(url);
        }

        public Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
        {
            var relativePath = fileUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString());
            if (relativePath.StartsWith($"uploads{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
                relativePath = relativePath[$"uploads{Path.DirectorySeparatorChar}".Length..];

            var fullPath = Path.Combine(_environment.ContentRootPath, "Uploads", relativePath);

            if (File.Exists(fullPath))
                File.Delete(fullPath);

            return Task.CompletedTask;
        }
    }
}
