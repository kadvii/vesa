using eVisaPlatform.Application.DTOs.Report;
using eVisaPlatform.Application.Interfaces;
using eVisaPlatform.Domain.Enums;
using eVisaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eVisaPlatform.Infrastructure.Repositories;

/// <summary>
/// Runs efficient aggregate COUNT/SUM queries directly against the DbContext.
/// Lives in Infrastructure so it can reference AppDbContext without violating
/// Clean Architecture (Application layer stays free of EF Core).
/// </summary>
public class ReportRepository : IReportRepository
{
    private readonly AppDbContext _db;

    public ReportRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<AnalyticsReportDto> GetAnalyticsAsync()
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // --- Users ---
        var totalUsers    = await _db.Users.CountAsync();
        var newUsersMonth = await _db.Users.CountAsync(u => u.CreatedAt >= startOfMonth);

        // --- Visa Applications ---
        var totalApps    = await _db.VisaApplications.CountAsync();
        var pendingApps  = await _db.VisaApplications.CountAsync(a => a.Status == VisaStatus.Pending);
        var reviewApps   = await _db.VisaApplications.CountAsync(a => a.Status == VisaStatus.UnderReview);
        var approvedApps = await _db.VisaApplications.CountAsync(a => a.Status == VisaStatus.Approved);
        var rejectedApps = await _db.VisaApplications.CountAsync(a => a.Status == VisaStatus.Rejected);

        // --- Payments ---
        var totalPayments   = await _db.Payments.CountAsync();
        var totalRevenue    = await _db.Payments
            .Where(p => p.Status == PaymentStatus.Paid)
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;
        var pendingPayments = await _db.Payments.CountAsync(p => p.Status == PaymentStatus.Pending);
        var failedPayments  = await _db.Payments.CountAsync(p => p.Status == PaymentStatus.Failed);

        // --- Appointments ---
        var totalAppts     = await _db.Appointments.CountAsync();
        var upcomingAppts  = await _db.Appointments
            .CountAsync(a => a.Status == AppointmentStatus.Scheduled && a.ScheduledAt > now);
        var cancelledAppts = await _db.Appointments.CountAsync(a => a.Status == AppointmentStatus.Cancelled);

        // --- Documents ---
        var totalDocs = await _db.Documents.CountAsync();

        // --- Support ---
        var openTickets = await _db.SupportTickets.CountAsync(t => t.Status == TicketStatus.Open);

        return new AnalyticsReportDto
        {
            TotalUsers              = totalUsers,
            NewUsersThisMonth       = newUsersMonth,
            TotalApplications       = totalApps,
            PendingApplications     = pendingApps,
            UnderReviewApplications = reviewApps,
            ApprovedApplications    = approvedApps,
            RejectedApplications    = rejectedApps,
            TotalPayments           = totalPayments,
            TotalRevenue            = totalRevenue,
            PendingPayments         = pendingPayments,
            FailedPayments          = failedPayments,
            TotalAppointments       = totalAppts,
            UpcomingAppointments    = upcomingAppts,
            CancelledAppointments   = cancelledAppts,
            TotalDocuments          = totalDocs,
            OpenSupportTickets      = openTickets,
            GeneratedAt             = now
        };
    }
}
