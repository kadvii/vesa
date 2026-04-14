using eVisaPlatform.Application.DTOs.Visa;
using eVisaPlatform.Application.Common;

namespace eVisaPlatform.Application.Interfaces;

public interface IVisaApplicationService
{
    Task<ApiResponse<PagedResult<VisaApplicationResponseDto>>> GetAllAsync(int page, int pageSize);
    Task<ApiResponse<PagedResult<VisaApplicationResponseDto>>> GetMyRequestsAsync(
        Guid userId, int page, int pageSize);
    Task<ApiResponse<VisaApplicationResponseDto>> GetByIdAsync(Guid id, Guid requestingUserId, bool isAdmin);
    Task<ApiResponse<VisaApplicationResponseDto>> CreateAsync(Guid userId, CreateVisaApplicationDto dto);
    Task<ApiResponse<VisaApplicationResponseDto>> UpdateAsync(Guid id, Guid userId, UpdateVisaApplicationDto dto);
    Task<ApiResponse<VisaApplicationResponseDto>> ApproveAsync(Guid id, string reviewerEmail, ReviewVisaApplicationDto dto);
    Task<ApiResponse<VisaApplicationResponseDto>> RejectAsync(Guid id, string reviewerEmail, ReviewVisaApplicationDto dto);
    Task<VisaStatsDto> GetStatsAsync();
}
