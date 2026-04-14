using eVisaPlatform.Application.DTOs.Appointment;

namespace eVisaPlatform.Application.Interfaces;

public interface IAppointmentService
{
    /// <summary>Schedule a new interview appointment for a visa application.</summary>
    Task<AppointmentResponseDto> ScheduleAsync(Guid userId, CreateAppointmentDto dto);

    /// <summary>Retrieve appointment details by ID.</summary>
    Task<AppointmentResponseDto> GetByIdAsync(Guid id);

    /// <summary>Cancel an existing appointment (owner or admin).</summary>
    Task CancelAsync(Guid userId, Guid appointmentId, bool isAdmin = false);
}
