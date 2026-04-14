using AutoMapper;
using eVisaPlatform.Application.DTOs.Consultants;
using eVisaPlatform.Application.Interfaces;
using eVisaPlatform.Domain.Entities;

namespace eVisaPlatform.Application.Services;

public class TravelConsultantService : ITravelConsultantService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TravelConsultantService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper     = mapper;
    }

    // ── Admin ─────────────────────────────────────────────────────────────────

    public async Task<TravelConsultantResponseDto> AddConsultantAsync(CreateTravelConsultantDto dto)
    {
        var consultant = new TravelConsultant
        {
            Id       = Guid.NewGuid(),
            FullName = dto.FullName,
            Email    = dto.Email,
            Phone    = dto.Phone
        };

        await _unitOfWork.TravelConsultants.AddAsync(consultant);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<TravelConsultantResponseDto>(consultant);
    }

    public async Task AssignConsultantAsync(Guid consultantId, Guid applicationId)
    {
        var application = await _unitOfWork.VisaApplications.GetByIdAsync(applicationId)
                          ?? throw new KeyNotFoundException("Application not found.");

        _ = await _unitOfWork.TravelConsultants.GetByIdAsync(consultantId)
            ?? throw new KeyNotFoundException("Travel consultant not found.");

        application.TravelConsultantId = consultantId;
        _unitOfWork.VisaApplications.Update(application);
        await _unitOfWork.SaveChangesAsync();
    }

    // ── User ──────────────────────────────────────────────────────────────────

    public async Task<IEnumerable<TravelConsultantResponseDto>> GetConsultantsAsync()
    {
        var consultants = await _unitOfWork.TravelConsultants.GetAllAsync();
        return _mapper.Map<IEnumerable<TravelConsultantResponseDto>>(consultants);
    }

    public async Task<TravelConsultantResponseDto?> GetConsultantByIdAsync(Guid id)
    {
        var consultant = await _unitOfWork.TravelConsultants.GetByIdAsync(id);
        return consultant is null ? null : _mapper.Map<TravelConsultantResponseDto>(consultant);
    }

    /// <summary>
    /// Books a consultant against an existing visa application.
    /// Internally assigns the consultant (sets TravelConsultantId on the application).
    /// </summary>
    public async Task<BookConsultantResponseDto> BookSessionAsync(
        Guid consultantId, Guid userId, BookConsultantDto dto)
    {
        var application = await _unitOfWork.VisaApplications.GetByIdAsync(dto.ApplicationId)
                          ?? throw new KeyNotFoundException("Visa application not found.");

        if (application.UserId != userId)
            throw new UnauthorizedAccessException("You are not the owner of this application.");

        _ = await _unitOfWork.TravelConsultants.GetByIdAsync(consultantId)
            ?? throw new KeyNotFoundException("Consultant not found.");

        application.TravelConsultantId = consultantId;
        _unitOfWork.VisaApplications.Update(application);
        await _unitOfWork.SaveChangesAsync();

        return new BookConsultantResponseDto
        {
            Message       = "Consultant booked successfully.",
            ConsultantId  = consultantId,
            ApplicationId = dto.ApplicationId
        };
    }
}
