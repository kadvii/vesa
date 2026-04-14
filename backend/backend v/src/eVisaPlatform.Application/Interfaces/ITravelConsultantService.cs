using eVisaPlatform.Application.DTOs.Consultants;

namespace eVisaPlatform.Application.Interfaces;

public interface ITravelConsultantService
{
    // Admin operations
    Task<TravelConsultantResponseDto> AddConsultantAsync(CreateTravelConsultantDto dto);
    Task AssignConsultantAsync(Guid consultantId, Guid applicationId);

    // User operations
    Task<IEnumerable<TravelConsultantResponseDto>> GetConsultantsAsync();
    Task<TravelConsultantResponseDto?> GetConsultantByIdAsync(Guid id);
    Task<BookConsultantResponseDto> BookSessionAsync(Guid consultantId, Guid userId, BookConsultantDto dto);
}
