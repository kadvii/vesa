using eVisaPlatform.Application.DTOs.Report;
using eVisaPlatform.Application.Interfaces;

namespace eVisaPlatform.Application.Services;

/// <summary>
/// Delegates analytics aggregation to IReportRepository,
/// which is implemented in Infrastructure with full DbContext access.
/// </summary>
public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;

    public ReportService(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public Task<AnalyticsReportDto> GetAnalyticsAsync()
        => _reportRepository.GetAnalyticsAsync();
}
