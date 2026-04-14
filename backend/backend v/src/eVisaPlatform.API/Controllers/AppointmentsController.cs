using eVisaPlatform.Application.DTOs.Appointment;
using eVisaPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace eVisaPlatform.API.Controllers;

[ApiController]
[Route("api/appointments")]
[Authorize]
[Produces("application/json")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());

    private bool IsAdmin =>
        User.IsInRole("Admin") || User.IsInRole("Employee");

    /// <summary>
    /// POST /api/appointments
    /// Schedule a new visa interview appointment.
    /// Returns 201 Created with the appointment details.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Schedule([FromBody] CreateAppointmentDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _appointmentService.ScheduleAsync(CurrentUserId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// GET /api/appointments/{id}
    /// Retrieve full details of a specific appointment.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _appointmentService.GetByIdAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// DELETE /api/appointments/{id}
    /// Cancel an appointment. Owner or Admin/Employee can cancel.
    /// Returns 204 No Content on success.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        await _appointmentService.CancelAsync(CurrentUserId, id, IsAdmin);
        return NoContent();
    }
}
