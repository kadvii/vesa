namespace eVisaPlatform.Application.DTOs.Report;

/// <summary>Aggregated analytics data for the admin dashboard.</summary>
public class AnalyticsReportDto
{
    // Users
    public int TotalUsers { get; set; }
    public int NewUsersThisMonth { get; set; }

    // Applications
    public int TotalApplications { get; set; }
    public int PendingApplications { get; set; }
    public int UnderReviewApplications { get; set; }
    public int ApprovedApplications { get; set; }
    public int RejectedApplications { get; set; }

    // Payments
    public int TotalPayments { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PendingPayments { get; set; }
    public int FailedPayments { get; set; }

    // Appointments
    public int TotalAppointments { get; set; }
    public int UpcomingAppointments { get; set; }
    public int CancelledAppointments { get; set; }

    // Documents
    public int TotalDocuments { get; set; }

    // Support
    public int OpenSupportTickets { get; set; }

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
