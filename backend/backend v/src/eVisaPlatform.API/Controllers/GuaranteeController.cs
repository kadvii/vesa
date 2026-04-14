using eVisaPlatform.Application.DTOs.Guarantee;
using eVisaPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eVisaPlatform.API.Controllers;

/// <summary>
/// Admin-facing guarantee management dashboard.
/// User-facing guarantee creation/refund endpoints live on /api/visa/{id}/add-guarantee and /api/visa/{id}/refund
/// </summary>
[Route("api/guarantee")]
[ApiController]
[Authorize(Roles = "Admin,Employee")]
[Produces("application/json")]
public class GuaranteeController : ControllerBase
{
    private readonly IVisaGuaranteeService _guaranteeService;

    public GuaranteeController(IVisaGuaranteeService guaranteeService)
        => _guaranteeService = guaranteeService;

    /// <summary>Get all guarantee requests [Admin/Employee]</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _guaranteeService.GetAllAsync();
        return Ok(result);
    }

    /// <summary>Get a guarantee request by ID [Admin/Employee]</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _guaranteeService.GetByIdAsync(id);
        return Ok(result);
    }

    /// <summary>Set risk score and notes on a guarantee [Admin/Employee]</summary>
    [HttpPut("{id:guid}/evaluate")]
    public async Task<IActionResult> EvaluateRisk(Guid id, [FromBody] EvaluateGuaranteeDto dto)
    {
        var result = await _guaranteeService.EvaluateRiskAsync(id, dto);
        return Ok(result);
    }

    /// <summary>Approve a guarantee request [Admin/Employee]</summary>
    [HttpPut("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        await _guaranteeService.ApproveGuaranteeAsync(id);
        return NoContent();
    }

    /// <summary>Reject a guarantee request [Admin/Employee]</summary>
    [HttpPut("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id)
    {
        await _guaranteeService.RejectGuaranteeAsync(id);
        return NoContent();
    }
}
