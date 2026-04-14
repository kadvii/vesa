using eVisaPlatform.Application.DTOs.AuditLog;
using eVisaPlatform.Domain.Entities;

namespace eVisaPlatform.Application.Interfaces;

public interface IAuditLogService
{
    /// <summary>Query audit logs with optional filtering by user, date range, action, or entity.</summary>
    Task<IEnumerable<AuditLogResponseDto>> GetLogsAsync(AuditLogFilterDto filter);

    /// <summary>Record a new audit event. Called internally from services.</summary>
    Task LogAsync(AuditLog entry);
}
