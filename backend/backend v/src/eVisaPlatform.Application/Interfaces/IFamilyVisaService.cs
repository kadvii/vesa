using eVisaPlatform.Application.DTOs.Family;

namespace eVisaPlatform.Application.Interfaces;

public interface IFamilyVisaService
{
    Task<FamilyMemberResponseDto> AddFamilyMemberAsync(Guid userId, CreateFamilyMemberDto dto);
    Task<IEnumerable<FamilyMemberResponseDto>> GetFamilyMembersAsync(Guid userId, Guid applicationId);
    Task<FamilyMemberResponseDto> GetByIdAsync(Guid userId, Guid memberId);
    Task<IEnumerable<FamilyMemberResponseDto>> GetAllAsync();
    Task DeleteFamilyMemberAsync(Guid userId, Guid memberId);
}
