using eVisaPlatform.Application.DTOs.Report;

namespace eVisaPlatform.Application.Interfaces;

public interface IReportService
{
    /// <summary>Aggregate system-wide statistics for admin analytics dashboard.</summary>
    Task<AnalyticsReportDto> GetAnalyticsAsync();
}
