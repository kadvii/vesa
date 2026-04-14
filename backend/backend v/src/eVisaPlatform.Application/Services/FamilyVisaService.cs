using AutoMapper;
using eVisaPlatform.Application.Common;
using eVisaPlatform.Application.DTOs.Family;
using eVisaPlatform.Application.Interfaces;
using eVisaPlatform.Domain.Entities;

namespace eVisaPlatform.Application.Services;

public class FamilyVisaService : IFamilyVisaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public FamilyVisaService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<FamilyMemberResponseDto> AddFamilyMemberAsync(Guid userId, CreateFamilyMemberDto dto)
    {
        var application = await _unitOfWork.VisaApplications.GetByIdAsync(dto.ApplicationId)
            ?? throw new KeyNotFoundException("Visa application not found.");
        if (application.UserId != userId)
            throw new UnauthorizedAccessException("You are not authorized to modify this application.");

        var member = new FamilyMember
        {
            Id = Guid.NewGuid(),
            ApplicationId = dto.ApplicationId,
            FullName = dto.FullName,
            PassportNumber = dto.PassportNumber,
            Age = dto.Age,
            Relationship = dto.Relationship
        };

        await _unitOfWork.FamilyMembers.AddAsync(member);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<FamilyMemberResponseDto>(member);
    }

    public async Task<IEnumerable<FamilyMemberResponseDto>> GetFamilyMembersAsync(Guid userId, Guid applicationId)
    {
        var application = await _unitOfWork.VisaApplications.GetByIdAsync(applicationId)
            ?? throw new KeyNotFoundException("Visa application not found.");
        if (application.UserId != userId)
            throw new UnauthorizedAccessException("You are not authorized to view this application.");

        var members = await _unitOfWork.FamilyMembers.GetByApplicationIdAsync(applicationId);
        return _mapper.Map<IEnumerable<FamilyMemberResponseDto>>(members);
    }

    public async Task<FamilyMemberResponseDto> GetByIdAsync(Guid userId, Guid memberId)
    {
        var member = await _unitOfWork.FamilyMembers.GetByIdAsync(memberId);
        if (member == null)
            throw new KeyNotFoundException("Family member not found.");
        var application = await _unitOfWork.VisaApplications.GetByIdAsync(member.ApplicationId);
        if (application == null || application.UserId != userId)
            throw new UnauthorizedAccessException("You are not authorized to view this family member.");
        return _mapper.Map<FamilyMemberResponseDto>(member);
    }

    public async Task<IEnumerable<FamilyMemberResponseDto>> GetAllAsync()
    {
        var all = await _unitOfWork.FamilyMembers.GetAllAsync();
        return _mapper.Map<IEnumerable<FamilyMemberResponseDto>>(all);
    }

    public async Task DeleteFamilyMemberAsync(Guid userId, Guid memberId)
    {
        var member = await _unitOfWork.FamilyMembers.GetByIdAsync(memberId);
        if (member == null)
            throw new KeyNotFoundException("Family member not found.");

        var application = await _unitOfWork.VisaApplications.GetByIdAsync(member.ApplicationId);
        if (application == null || application.UserId != userId)
            throw new UnauthorizedAccessException("You are not authorized to delete this family member.");

        _unitOfWork.FamilyMembers.Delete(member);
        await _unitOfWork.SaveChangesAsync();
    }
}
