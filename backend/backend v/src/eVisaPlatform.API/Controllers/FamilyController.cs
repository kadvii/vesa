using eVisaPlatform.Application.DTOs.Family;
using eVisaPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace eVisaPlatform.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FamilyController : ControllerBase
{
    private readonly IFamilyVisaService _familyVisaService;

    public FamilyController(IFamilyVisaService familyVisaService)
    {
        _familyVisaService = familyVisaService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<IActionResult> AddFamilyMember([FromBody] CreateFamilyMemberDto dto)
    {
        var member = await _familyVisaService.AddFamilyMemberAsync(GetUserId(), dto);
        return Ok(member);
    }

    /// <summary>Get all family members for a specific application</summary>
    [HttpGet("{applicationId:guid}")]
    public async Task<IActionResult> GetFamilyMembers(Guid applicationId)
    {
        var members = await _familyVisaService.GetFamilyMembersAsync(GetUserId(), applicationId);
        return Ok(members);
    }

    /// <summary>Get a single family member by their ID</summary>
    [HttpGet("member/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var member = await _familyVisaService.GetByIdAsync(GetUserId(), id);
        return Ok(member);
    }

    /// <summary>Get all family members (Admin/Employee only)</summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> GetAll()
    {
        var members = await _familyVisaService.GetAllAsync();
        return Ok(members);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteFamilyMember(Guid id)
    {
        await _familyVisaService.DeleteFamilyMemberAsync(GetUserId(), id);
        return NoContent();
    }
}
