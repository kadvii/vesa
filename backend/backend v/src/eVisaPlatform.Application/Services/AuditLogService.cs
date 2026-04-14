using AutoMapper;
using eVisaPlatform.Application.DTOs.AuditLog;
using eVisaPlatform.Application.Interfaces;
using eVisaPlatform.Domain.Entities;

namespace eVisaPlatform.Application.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AuditLogService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>
    /// Returns audit logs filtered by optional criteria (userId, email, action,
    /// entity name, date range) with pagination. Used for compliance reporting.
    /// </summary>
    public async Task<IEnumerable<AuditLogResponseDto>> GetLogsAsync(AuditLogFilterDto filter)
    {
        var logs = await _unitOfWork.AuditLogs.GetFilteredAsync(filter);
        return _mapper.Map<IEnumerable<AuditLogResponseDto>>(logs);
    }

    /// <summary>Persist a new audit log entry. Called from service layer on sensitive operations.</summary>
    public async Task LogAsync(AuditLog entry)
    {
        entry.Id = Guid.NewGuid();
        entry.CreatedAt = DateTime.UtcNow;
        await _unitOfWork.AuditLogs.AddAsync(entry);
        await _unitOfWork.SaveChangesAsync();
    }
}
