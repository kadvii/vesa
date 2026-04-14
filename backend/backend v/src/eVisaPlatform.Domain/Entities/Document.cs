using eVisaPlatform.Domain.Enums;

namespace eVisaPlatform.Domain.Entities;

public class Document
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DocumentType FileType { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public VisaApplication Application { get; set; } = null!;
}
