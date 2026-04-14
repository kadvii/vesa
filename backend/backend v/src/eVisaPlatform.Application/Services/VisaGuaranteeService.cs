using AutoMapper;
using eVisaPlatform.Application.DTOs.Guarantee;
using eVisaPlatform.Application.Interfaces;
using eVisaPlatform.Domain.Entities;
using eVisaPlatform.Domain.Enums;

namespace eVisaPlatform.Application.Services;

public class VisaGuaranteeService : IVisaGuaranteeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public VisaGuaranteeService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper     = mapper;
    }

    // ── Admin ─────────────────────────────────────────────────────────────────

    public async Task<IEnumerable<GuaranteeRequestResponseDto>> GetAllAsync()
    {
        var all = await _unitOfWork.GuaranteeRequests.GetAllAsync();
        return _mapper.Map<IEnumerable<GuaranteeRequestResponseDto>>(all);
    }

    public async Task<GuaranteeRequestResponseDto> GetByIdAsync(Guid guaranteeId)
    {
        var guarantee = await _unitOfWork.GuaranteeRequests.GetByIdAsync(guaranteeId)
                        ?? throw new KeyNotFoundException("Guarantee request not found.");
        return _mapper.Map<GuaranteeRequestResponseDto>(guarantee);
    }

    public async Task<GuaranteeRequestResponseDto> EvaluateRiskAsync(
        Guid guaranteeId, EvaluateGuaranteeDto dto)
    {
        var guarantee = await _unitOfWork.GuaranteeRequests.GetByIdAsync(guaranteeId)
                        ?? throw new KeyNotFoundException("Guarantee request not found.");

        guarantee.RiskScore = dto.RiskScore;
        if (!string.IsNullOrWhiteSpace(dto.Notes))
            guarantee.Notes = dto.Notes;

        _unitOfWork.GuaranteeRequests.Update(guarantee);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<GuaranteeRequestResponseDto>(guarantee);
    }

    public async Task ApproveGuaranteeAsync(Guid guaranteeId)
    {
        var guarantee = await _unitOfWork.GuaranteeRequests.GetByIdAsync(guaranteeId)
                        ?? throw new KeyNotFoundException("Guarantee request not found.");

        guarantee.Status = GuaranteeStatus.Approved;
        _unitOfWork.GuaranteeRequests.Update(guarantee);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RejectGuaranteeAsync(Guid guaranteeId)
    {
        var guarantee = await _unitOfWork.GuaranteeRequests.GetByIdAsync(guaranteeId)
                        ?? throw new KeyNotFoundException("Guarantee request not found.");

        guarantee.Status = GuaranteeStatus.Rejected;
        _unitOfWork.GuaranteeRequests.Update(guarantee);
        await _unitOfWork.SaveChangesAsync();
    }

    // ── User (via visa application ID) ───────────────────────────────────────

    /// <summary>
    /// POST /api/visa/{applicationId}/add-guarantee
    /// Adds a guarantee request to an existing visa application owned by the user.
    /// </summary>
    public async Task<GuaranteeRequestResponseDto> AddGuaranteeToVisaAsync(
        Guid applicationId, Guid userId)
    {
        var application = await _unitOfWork.VisaApplications.GetByIdAsync(applicationId)
                          ?? throw new KeyNotFoundException("Visa application not found.");

        if (application.UserId != userId)
            throw new UnauthorizedAccessException("You are not the owner of this application.");

        var existing = await _unitOfWork.GuaranteeRequests.GetByApplicationIdAsync(applicationId);
        if (existing != null)
            throw new InvalidOperationException("A guarantee request already exists for this application.");

        var guarantee = new GuaranteeRequest
        {
            Id            = Guid.NewGuid(),
            ApplicationId = applicationId,
            RiskScore     = 0,
            Status        = GuaranteeStatus.Pending,
            Notes         = "Awaiting risk evaluation."
        };

        await _unitOfWork.GuaranteeRequests.AddAsync(guarantee);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<GuaranteeRequestResponseDto>(guarantee);
    }

    /// <summary>
    /// POST /api/visa/{applicationId}/refund
    /// Requests a refund for a guarantee. Only allowed when the visa was Rejected.
    /// </summary>
    public async Task<GuaranteeRequestResponseDto> RequestRefundAsync(
        Guid applicationId, Guid userId)
    {
        var application = await _unitOfWork.VisaApplications.GetByIdAsync(applicationId)
                          ?? throw new KeyNotFoundException("Visa application not found.");

        if (application.UserId != userId)
            throw new UnauthorizedAccessException("You are not the owner of this application.");

        var guarantee = await _unitOfWork.GuaranteeRequests.GetByApplicationIdAsync(applicationId)
                        ?? throw new KeyNotFoundException("No guarantee found for this application.");

        if (application.Status != VisaStatus.Rejected)
            throw new InvalidOperationException(
                "Refund can only be requested when the visa application has been Rejected.");

        if (guarantee.Status == GuaranteeStatus.Refunded)
            throw new InvalidOperationException("Refund has already been processed.");

        guarantee.Status = GuaranteeStatus.Refunded;
        guarantee.Notes  = "Refund requested after visa rejection.";
        _unitOfWork.GuaranteeRequests.Update(guarantee);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<GuaranteeRequestResponseDto>(guarantee);
    }
}
