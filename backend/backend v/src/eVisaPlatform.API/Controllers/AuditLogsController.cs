using eVisaPlatform.Application.DTOs.AuditLog;
using eVisaPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eVisaPlatform.API.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    /// <summary>
    /// GET /api/audit-logs
    /// Retrieve filtered, paginated audit logs for compliance.
    /// Supports filtering by: userId, userEmail, action, entityName, from/to dates.
    /// Query params: ?userId=&amp;userEmail=&amp;action=&amp;entityName=&amp;from=&amp;to=&amp;page=1&amp;pageSize=50
    /// Admin only.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetLogs([FromQuery] AuditLogFilterDto filter)
    {
        // Clamp page size to prevent abuse
        filter.PageSize = Math.Clamp(filter.PageSize, 1, 200);

        var result = await _auditLogService.GetLogsAsync(filter);
        return Ok(result);
    }
}
