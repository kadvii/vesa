using eVisaPlatform.Application.DTOs.Visa;
using eVisaPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace eVisaPlatform.API.Controllers;

/// <summary>
/// Visa application lifecycle: apply, track, approve/reject.
/// Route convention: /api/visa
/// </summary>
[ApiController]
[Route("api/visa")]
[Authorize]
[Produces("application/json")]
public class VisaController : ControllerBase
{
    private readonly IVisaApplicationService _visaService;
    private readonly IVisaGuaranteeService _guaranteeService;

    public VisaController(
        IVisaApplicationService visaService,
        IVisaGuaranteeService guaranteeService)
    {
        _visaService    = visaService;
        _guaranteeService = guaranteeService;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private Guid CurrentUserId
    {
        get
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                      ?? User.FindFirstValue("sub");
            if (!Guid.TryParse(raw, out var id) || id == Guid.Empty)
                throw new UnauthorizedAccessException("Missing or invalid user identifier in the access token.");
            return id;
        }
    }

    private string CurrentUserEmail =>
        User.FindFirstValue(ClaimTypes.Email)
        ?? User.FindFirstValue("email")
        ?? string.Empty;

    private bool IsAdminOrEmployee =>
        User.IsInRole("Admin") || User.IsInRole("Employee");

    // ── Admin: list all ───────────────────────────────────────────────────────
    /// <summary>Get all visa applications (paginated) [Admin only]</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _visaService.GetAllAsync(page, pageSize);
        return Ok(result);
    }

    // ── GET my own requests ───────────────────────────────────────────────────
    /// <summary>Get the authenticated user's own visa applications</summary>
    [HttpGet("my-requests")]
    [Authorize]
    public async Task<IActionResult> GetMyRequests([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _visaService.GetMyRequestsAsync(CurrentUserId, page, pageSize);
        return Ok(result);
    }

    // ── STATS (Admin/Employee) ────────────────────────────────────────────────
    /// <summary>Aggregated visa application counts by status [Admin or Employee]</summary>
    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _visaService.GetStatsAsync();
        return Ok(stats);
    }

    // ── GET single (owner or admin) ───────────────────────────────────────────
    /// <summary>Get a single visa application by ID — owner or Admin/Employee only</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _visaService.GetByIdAsync(id, CurrentUserId, IsAdminOrEmployee);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // ── APPLY ─────────────────────────────────────────────────────────────────
    /// <summary>Submit a new visa application</summary>
    [HttpPost("apply")]
    [Authorize]
    public async Task<IActionResult> Apply([FromBody] CreateVisaApplicationDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _visaService.CreateAsync(CurrentUserId, dto);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result)
            : BadRequest(result);
    }

    // ── UPDATE ────────────────────────────────────────────────────────────────
    /// <summary>Update a Pending visa application [Owner only]</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVisaApplicationDto dto)
    {
        var result = await _visaService.UpdateAsync(id, CurrentUserId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── APPROVE (Admin or Employee) ───────────────────────────────────────────
    /// <summary>Approve a pending visa application [Admin or Employee]</summary>
    [HttpPut("{id:guid}/approve")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ReviewVisaApplicationDto dto)
    {
        var result = await _visaService.ApproveAsync(id, CurrentUserEmail, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── REJECT (Admin or Employee) ────────────────────────────────────────────
    /// <summary>Reject a pending visa application [Admin or Employee]</summary>
    [HttpPut("{id:guid}/reject")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] ReviewVisaApplicationDto dto)
    {
        var result = await _visaService.RejectAsync(id, CurrentUserEmail, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── PATCH STATUS (Admin or Employee) — unified single endpoint ─────────────────────────
    /// <summary>
    /// Unified status update: PATCH /api/visa/{id}/status
    /// Body: { "status": "Approved" | "Rejected", "notes": "optional" }
    /// Secured with [Authorize(Roles = "Admin,Employee")].
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateVisaStatusDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Status))
            return BadRequest(new { success = false, message = "Status field is required." });

        var reviewDto = new ReviewVisaApplicationDto { Notes = dto.Notes };

        var result = dto.Status.Trim() switch
        {
            "Approved" => await _visaService.ApproveAsync(id, CurrentUserEmail, reviewDto),
            "Rejected" => await _visaService.RejectAsync(id, CurrentUserEmail, reviewDto),
            _          => null
        };

        if (result is null)
            return BadRequest(new { success = false, message = "Status must be 'Approved' or 'Rejected'." });

        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── STATUS (public lightweight check for applicant) ───────────────────────
    /// <summary>Lightweight status check for an applicant — returns status, dates, notes</summary>
    [HttpGet("{id:guid}/status")]
    public async Task<IActionResult> GetStatus(Guid id)
    {
        var result = await _visaService.GetByIdAsync(id, CurrentUserId, IsAdminOrEmployee);
        if (!result.Success) return NotFound(result);

        return Ok(new
        {
            result.Data!.Id,
            result.Data.Status,
            result.Data.SubmissionDate,
            result.Data.ReviewDate,
            result.Data.ReviewedBy,
            result.Data.Notes
        });
    }

    // ── GUARANTEE ─────────────────────────────────────────────────────────────

    /// <summary>Add a guarantee to a visa application [Owner only]</summary>
    [HttpPost("{id:guid}/add-guarantee")]
    public async Task<IActionResult> AddGuarantee(Guid id)
    {
        var result = await _guaranteeService.AddGuaranteeToVisaAsync(id, CurrentUserId);
        return CreatedAtAction(nameof(GetById), new { id }, result);
    }

    /// <summary>Request a refund on the guarantee when visa is Rejected [Owner only]</summary>
    [HttpPost("{id:guid}/refund")]
    public async Task<IActionResult> RequestRefund(Guid id)
    {
        var result = await _guaranteeService.RequestRefundAsync(id, CurrentUserId);
        return Ok(result);
    }
}
