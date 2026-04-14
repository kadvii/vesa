using eVisaPlatform.Domain.Enums;

namespace eVisaPlatform.Domain.Entities;

/// <summary>Represents a scheduled visa interview appointment.</summary>
public class Appointment
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public Guid UserId { get; set; }

    public DateTime ScheduledAt { get; set; }
    public string Location { get; set; } = string.Empty;
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public VisaApplication Application { get; set; } = null!;
    public User User { get; set; } = null!;
}
