using eVisaPlatform.Application.DTOs.Guarantee;

namespace eVisaPlatform.Application.Interfaces;

public interface IVisaGuaranteeService
{
    // Admin operations
    Task<IEnumerable<GuaranteeRequestResponseDto>> GetAllAsync();
    Task<GuaranteeRequestResponseDto> GetByIdAsync(Guid guaranteeId);
    Task<GuaranteeRequestResponseDto> EvaluateRiskAsync(Guid guaranteeId, EvaluateGuaranteeDto dto);
    Task ApproveGuaranteeAsync(Guid guaranteeId);
    Task RejectGuaranteeAsync(Guid guaranteeId);

    // User operations (via visa application ID)
    Task<GuaranteeRequestResponseDto> AddGuaranteeToVisaAsync(Guid applicationId, Guid userId);
    Task<GuaranteeRequestResponseDto> RequestRefundAsync(Guid applicationId, Guid userId);
}
