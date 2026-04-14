using System.ComponentModel.DataAnnotations;

namespace eVisaPlatform.Application.DTOs.Appointment;

/// <summary>Input for scheduling a visa interview appointment.</summary>
public class CreateAppointmentDto
{
    [Required] public Guid ApplicationId { get; set; }

    [Required] public DateTime ScheduledAt { get; set; }

    [Required, MaxLength(500)] public string Location { get; set; } = string.Empty;

    [MaxLength(1000)] public string? Notes { get; set; }
}

/// <summary>API response shape for an appointment.</summary>
public class AppointmentResponseDto
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public Guid UserId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
