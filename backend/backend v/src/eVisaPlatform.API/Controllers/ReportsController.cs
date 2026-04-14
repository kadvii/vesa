using eVisaPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eVisaPlatform.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// GET /api/reports/analytics
    /// Returns aggregated statistics: user counts, application statuses,
    /// payment totals, appointment counts, document counts, open tickets.
    /// Admin only.
    /// </summary>
    [HttpGet("analytics")]
    public async Task<IActionResult> GetAnalytics()
    {
        var result = await _reportService.GetAnalyticsAsync();
        return Ok(result);
    }
}
