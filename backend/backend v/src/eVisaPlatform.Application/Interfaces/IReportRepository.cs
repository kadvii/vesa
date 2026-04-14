using eVisaPlatform.Application.DTOs.Report;

namespace eVisaPlatform.Application.Interfaces;

/// <summary>
/// Provides raw aggregate data for analytics.
/// Implemented in Infrastructure where DbContext is accessible.
/// </summary>
public interface IReportRepository
{
    Task<AnalyticsReportDto> GetAnalyticsAsync();
}
