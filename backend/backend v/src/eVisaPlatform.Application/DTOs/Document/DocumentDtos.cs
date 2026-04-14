using eVisaPlatform.Domain.Enums;

namespace eVisaPlatform.Application.DTOs.Document;

public class DocumentResponseDto
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}

public class UploadDocumentDto
{
    public Guid ApplicationId { get; set; }
    public DocumentType FileType { get; set; }
}
