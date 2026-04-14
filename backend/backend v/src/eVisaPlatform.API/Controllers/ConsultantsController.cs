using eVisaPlatform.Application.DTOs.Consultants;
using eVisaPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using System.Security.Claims;

namespace eVisaPlatform.API.Controllers;

/// <summary>
/// Travel Consultants: public listing for users, management for Admin/Employee.
/// Route: /api/consultants
/// </summary>
[Route("api/consultants")]
[ApiController]
[Authorize]
[Produces("application/json")]
public class ConsultantsController : ControllerBase
{
    private readonly ITravelConsultantService _consultantService;

    public ConsultantsController(ITravelConsultantService consultantService)
        => _consultantService = consultantService;

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User.FindFirstValue("sub")
                   ?? Guid.Empty.ToString());

    // ── User-accessible ───────────────────────────────────────────────────────

    /// <summary>List all available travel consultants [Any authenticated user]</summary>
    [OutputCache(Duration = 120)]
    [HttpGet]
    public async Task<IActionResult> GetConsultants()
    {
        var consultants = await _consultantService.GetConsultantsAsync();
        return Ok(consultants);
    }

    /// <summary>Get a single consultant by ID [Any authenticated user]</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var consultant = await _consultantService.GetConsultantByIdAsync(id);
        return consultant is null
            ? NotFound(new { message = "Consultant not found." })
            : Ok(consultant);
    }

    /// <summary>Book a session with a consultant [Any authenticated user]</summary>
    [HttpPost("{id:guid}/book")]
    public async Task<IActionResult> BookSession(Guid id, [FromBody] BookConsultantDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _consultantService.BookSessionAsync(id, CurrentUserId, dto);
        return Ok(result);
    }

    // ── Admin / Employee only ─────────────────────────────────────────────────

    /// <summary>Add a new consultant [Admin/Employee only]</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> AddConsultant([FromBody] CreateTravelConsultantDto dto)
    {
        var consultant = await _consultantService.AddConsultantAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = consultant.Id }, consultant);
    }

    /// <summary>Assign a consultant to a visa application [Admin/Employee only]</summary>
    [HttpPut("{id:guid}/assign/{applicationId:guid}")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> AssignConsultant(Guid id, Guid applicationId)
    {
        await _consultantService.AssignConsultantAsync(id, applicationId);
        return NoContent();
    }
}
