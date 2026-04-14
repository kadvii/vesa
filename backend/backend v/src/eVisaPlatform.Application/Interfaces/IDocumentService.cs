using eVisaPlatform.Application.DTOs.Document;
using eVisaPlatform.Application.Common;
using Microsoft.AspNetCore.Http;

namespace eVisaPlatform.Application.Interfaces;

public interface IDocumentService
{
    Task<ApiResponse<DocumentResponseDto>> UploadAsync(IFormFile file, UploadDocumentDto dto, Guid requestingUserId);
    Task<ApiResponse<IEnumerable<DocumentResponseDto>>> GetAllAsync();
    Task<ApiResponse<IEnumerable<DocumentResponseDto>>> GetByApplicationIdAsync(Guid applicationId);
    Task<ApiResponse<DocumentResponseDto>> GetByIdAsync(Guid id);
    Task<ApiResponse> DeleteAsync(Guid id);
}
