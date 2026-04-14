using AutoMapper;
using eVisaPlatform.Application.Common;
using eVisaPlatform.Application.DTOs.Document;
using eVisaPlatform.Application.Interfaces;
using eVisaPlatform.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace eVisaPlatform.Application.Services;

public class DocumentService : IDocumentService
{
    private readonly IUnitOfWork       _unitOfWork;
    private readonly IMapper           _mapper;
    private readonly IDateTimeProvider _clock;
    private readonly string            _uploadsPath;

    // ── Allowed file extensions ───────────────────────────────────────────────
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".jpg", ".jpeg", ".png" };

    // ── Max 5 MB ──────────────────────────────────────────────────────────────
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    /// <summary>
    /// Magic bytes (file signatures) for each accepted MIME type.
    /// Checking the actual binary header prevents "rename a .exe to .jpg" attacks.
    /// </summary>
    private static readonly Dictionary<string, byte[][]> MagicBytes = new(StringComparer.OrdinalIgnoreCase)
    {
        // JPEG — starts with FF D8 FF
        [".jpg"]  = [new byte[] { 0xFF, 0xD8, 0xFF }],
        [".jpeg"] = [new byte[] { 0xFF, 0xD8, 0xFF }],
        // PNG — starts with 89 50 4E 47 0D 0A 1A 0A
        [".png"]  = [new byte[] { 0x89, 0x50, 0x4E, 0x47 }],
        // PDF — starts with %PDF  (25 50 44 46)
        [".pdf"]  = [new byte[] { 0x25, 0x50, 0x44, 0x46 }],
    };

    public DocumentService(
        IUnitOfWork       unitOfWork,
        IMapper           mapper,
        IDateTimeProvider clock)
    {
        _unitOfWork  = unitOfWork;
        _mapper      = mapper;
        _clock       = clock;

        // Resolve <contentRoot>/wwwroot/uploads  (works whether running from bin/ or IDE)
        var webRoot = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "uploads");
        Directory.CreateDirectory(webRoot);
        _uploadsPath = webRoot;
    }

    // ── UPLOAD ────────────────────────────────────────────────────────────────
    public async Task<ApiResponse<DocumentResponseDto>> UploadAsync(
        IFormFile file, UploadDocumentDto dto, Guid requestingUserId)
    {
        // 1. Basic null / empty check
        if (file is null || file.Length == 0)
            return ApiResponse<DocumentResponseDto>.Fail("لم يتم إرفاق أي ملف.");

        // 2. Size guard (≤ 5 MB)
        if (file.Length > MaxFileSizeBytes)
            return ApiResponse<DocumentResponseDto>.Fail(
                "حجم الملف تجاوز الحد المسموح به (5 ميغابايت).");

        // 3. Extension whitelist
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return ApiResponse<DocumentResponseDto>.Fail(
                "نوع الملف غير مقبول. الأنواع المسموح بها: PDF، JPG، PNG.");

        // 4. Magic-byte validation — read first 8 bytes to verify real file type
        if (!await HasValidMagicBytesAsync(file, ext))
            return ApiResponse<DocumentResponseDto>.Fail(
                "محتوى الملف لا يتطابق مع امتداده. قد يكون الملف تالفاً أو ضاراً.");

        // 5. Ownership check — the application must belong to the requesting user
        //    (Admins/Employees bypass this check via the controller)
        var app = await _unitOfWork.VisaApplications.GetByIdAsync(dto.ApplicationId);
        if (app is null)
            return ApiResponse<DocumentResponseDto>.Fail("الطلب غير موجود.");

        if (app.UserId != requestingUserId)
            return ApiResponse<DocumentResponseDto>.Fail(
                "غير مصرح لك برفع مستندات لهذا الطلب.");

        // 6. Sanitise the original filename before storing it in the DB
        //    Strip any directory components to prevent path-traversal.
        var safeOriginalName = Path.GetFileName(file.FileName);

        // 7. Save with a GUID name — the extension is the ONLY part kept from user input
        var uniqueName = $"{Guid.NewGuid()}{ext}";
        var fullPath   = Path.Combine(_uploadsPath, uniqueName);

        await using (var stream = new FileStream(
                         fullPath, FileMode.Create,
                         FileAccess.Write, FileShare.None,
                         bufferSize: 81920, useAsync: true))
        {
            await file.CopyToAsync(stream);
        }

        // 8. Persist record
        var document = new Document
        {
            Id            = Guid.NewGuid(),
            ApplicationId = dto.ApplicationId,
            FileName      = safeOriginalName,          // display name only
            FilePath      = $"/uploads/{uniqueName}",  // physical path (never user-supplied)
            FileType      = dto.FileType,
            UploadedAt    = _clock.UtcNow
        };

        await _unitOfWork.Documents.AddAsync(document);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<DocumentResponseDto>.Ok(
            _mapper.Map<DocumentResponseDto>(document),
            "تم رفع المستند بنجاح.");
    }

    // ── READ ──────────────────────────────────────────────────────────────────
    public async Task<ApiResponse<IEnumerable<DocumentResponseDto>>> GetByApplicationIdAsync(
        Guid applicationId)
    {
        var docs = await _unitOfWork.Documents.GetByApplicationIdAsync(applicationId);
        return ApiResponse<IEnumerable<DocumentResponseDto>>.Ok(
            _mapper.Map<IEnumerable<DocumentResponseDto>>(docs));
    }

    public async Task<ApiResponse<IEnumerable<DocumentResponseDto>>> GetAllAsync()
    {
        var docs = await _unitOfWork.Documents.GetAllAsync();
        return ApiResponse<IEnumerable<DocumentResponseDto>>.Ok(
            _mapper.Map<IEnumerable<DocumentResponseDto>>(docs));
    }

    public async Task<ApiResponse<DocumentResponseDto>> GetByIdAsync(Guid id)
    {
        var doc = await _unitOfWork.Documents.GetByIdAsync(id);
        if (doc is null)
            return ApiResponse<DocumentResponseDto>.Fail("المستند غير موجود.");
        return ApiResponse<DocumentResponseDto>.Ok(_mapper.Map<DocumentResponseDto>(doc));
    }

    // ── DELETE ────────────────────────────────────────────────────────────────
    public async Task<ApiResponse> DeleteAsync(Guid id)
    {
        var doc = await _unitOfWork.Documents.GetByIdAsync(id);
        if (doc is null)
            return ApiResponse.Fail("المستند غير موجود.");

        // Delete physical file — build path from the stored relative URL, never from user input
        var relativePath = doc.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "wwwroot", relativePath);

        if (System.IO.File.Exists(fullPath))
            System.IO.File.Delete(fullPath);

        _unitOfWork.Documents.Delete(doc);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse.Ok("تم حذف المستند.");
    }

    // ── Private helpers ───────────────────────────────────────────────────────
    /// <summary>
    /// Reads the first bytes of the uploaded stream and compares them against
    /// the known magic-byte signatures for the declared extension.
    /// This prevents disguised executables from slipping through the extension filter.
    /// </summary>
    private static async Task<bool> HasValidMagicBytesAsync(IFormFile file, string ext)
    {
        if (!MagicBytes.TryGetValue(ext, out var signatures))
            return false; // unknown extension

        var maxHeader = signatures.Max(s => s.Length);
        var header    = new byte[maxHeader];

        await using var stream = file.OpenReadStream();
        var bytesRead = await stream.ReadAsync(header.AsMemory(0, maxHeader));

        return signatures.Any(sig =>
            bytesRead >= sig.Length &&
            header.Take(sig.Length).SequenceEqual(sig));
    }
}
