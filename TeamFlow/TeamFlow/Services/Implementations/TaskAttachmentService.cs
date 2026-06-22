using TeamFlow.Entities;
using TeamFlow.Repositories.Interfaces;
using TeamFlow.Services.Interfaces;
using TeamFlow.Services.Results;

namespace TeamFlow.Services.Implementations
{
    public class TaskAttachmentService : ITaskAttachmentService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ITaskAttachmentRepository _attachmentRepository;
        private readonly IFileStorageService _fileStorageService;

        public TaskAttachmentService(
            ITaskRepository taskRepository,
            ITaskAttachmentRepository attachmentRepository,
            IFileStorageService fileStorageService)
        {
            _taskRepository = taskRepository;
            _attachmentRepository = attachmentRepository;
            _fileStorageService = fileStorageService;
        }

        public async Task<IEnumerable<TaskAttachment>> GetByTaskIdAsync(int taskId)
            => await _attachmentRepository.GetByTaskIdAsync(taskId);

        public async Task<ServiceResult<TaskAttachment>> UploadAsync(int taskId, IFormFile file, CancellationToken cancellationToken = default)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task is null)
                return ServiceResult<TaskAttachment>.Failure("Task not found");

            var savedFile = await _fileStorageService.SaveAsync(file, $"tasks/{taskId}", cancellationToken);
            if (!savedFile.Succeeded || savedFile.Data is null)
                return ServiceResult<TaskAttachment>.Failure(savedFile.Error ?? "Upload failed");

            var attachment = new TaskAttachment
            {
                TaskId = taskId,
                FileUrl = savedFile.Data,
                FileName = Path.GetFileName(file.FileName),
                FileType = file.ContentType,
                FileSize = file.Length,
                CreatedAt = DateTime.UtcNow
            };

            await _attachmentRepository.AddAsync(attachment);
            await _attachmentRepository.SaveAsync();

            return ServiceResult<TaskAttachment>.Success(attachment);
        }

        public async Task<ServiceResult<object>> DeleteAsync(int taskId, int attachmentId, CancellationToken cancellationToken = default)
        {
            var attachment = await _attachmentRepository.GetByIdAsync(attachmentId);
            if (attachment is null || attachment.TaskId != taskId)
                return ServiceResult<object>.Failure("Attachment not found");

            _attachmentRepository.Delete(attachment);
            await _attachmentRepository.SaveAsync();
            await _fileStorageService.DeleteAsync(attachment.FileUrl, cancellationToken);

            return ServiceResult<object>.Success();
        }
    }
}
