public class TaskAttachmentDto
{
    public int Id { get; set; }
    public int TaskId { get; set; }

    public string FileUrl { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DateTime CreatedAt { get; set; }
}