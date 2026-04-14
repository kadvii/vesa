using eVisaPlatform.Application.DTOs.AuditLog;
using eVisaPlatform.Application.Interfaces;
using eVisaPlatform.Domain.Entities;
using eVisaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eVisaPlatform.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email)
        => await _dbSet.AsNoTracking().FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

    public async Task<bool> EmailExistsAsync(string email)
        => await _dbSet.AnyAsync(u => u.Email.ToLower() == email.ToLower());
}

public class VisaApplicationRepository : GenericRepository<VisaApplication>, IVisaApplicationRepository
{
    public VisaApplicationRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<VisaApplication>> GetAllWithDetailsAsync()
        => await _dbSet.AsNoTracking().Include(v => v.User).Include(v => v.Documents).ToListAsync();

    public async Task<VisaApplication?> GetByIdWithDetailsAsync(Guid id)
        => await _dbSet.Include(v => v.User).Include(v => v.Documents).FirstOrDefaultAsync(v => v.Id == id);

    public async Task<IEnumerable<VisaApplication>> GetByUserIdAsync(Guid userId)
        => await _dbSet
            .AsNoTracking()
            .Include(v => v.User)
            .Include(v => v.Documents)
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.SubmissionDate)
            .ToListAsync();

    public async Task<(IReadOnlyList<VisaApplication> Items, int TotalCount)>
        GetAllWithDetailsPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        var baseQuery = _dbSet.AsNoTracking().OrderByDescending(v => v.SubmissionDate);
        var total = await baseQuery.CountAsync(cancellationToken);
        var items = await baseQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(v => v.User)
            .Include(v => v.Documents)
            .ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task<(IReadOnlyList<VisaApplication> Items, int TotalCount)>
        GetByUserIdPagedAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        var baseQuery = _dbSet
            .AsNoTracking()
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.SubmissionDate);
        var total = await baseQuery.CountAsync(cancellationToken);
        var items = await baseQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(v => v.User)
            .Include(v => v.Documents)
            .ToListAsync(cancellationToken);
        return (items, total);
    }
}

public class DocumentRepository : GenericRepository<Document>, IDocumentRepository
{
    public DocumentRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Document>> GetByApplicationIdAsync(Guid applicationId)
        => await _dbSet.AsNoTracking().Where(d => d.ApplicationId == applicationId).ToListAsync();
}

public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(AppDbContext context) : base(context) { }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
        => await _dbSet.FirstOrDefaultAsync(r => r.Token == token);

    public async Task RevokeAllUserTokensAsync(Guid userId)
    {
        var tokens = await _dbSet.Where(r => r.UserId == userId && !r.IsRevoked).ToListAsync();
        tokens.ForEach(t => t.IsRevoked = true);
    }

    public async Task<int> DeleteExpiredAndRevokedAsync(CancellationToken cancellationToken = default)
    {
        var utc = DateTime.UtcNow;
        return await _dbSet
            .Where(r => r.ExpiresAt < utc || r.IsRevoked)
            .ExecuteDeleteAsync(cancellationToken);
    }
}

public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    public NotificationRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId)
        => await _dbSet.AsNoTracking().Where(n => n.UserId == userId).OrderByDescending(n => n.CreatedAt).ToListAsync();
}

public class FamilyMemberRepository : GenericRepository<FamilyMember>, IFamilyMemberRepository
{
    public FamilyMemberRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<FamilyMember>> GetByApplicationIdAsync(Guid applicationId)
        => await _dbSet.AsNoTracking().Where(f => f.ApplicationId == applicationId).ToListAsync();
}

public class SupportTicketRepository : GenericRepository<SupportTicket>, ISupportTicketRepository
{
    public SupportTicketRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<SupportTicket>> GetByUserIdAsync(Guid userId)
        => await _dbSet.AsNoTracking().Where(t => t.UserId == userId).OrderByDescending(t => t.CreatedAt).ToListAsync();

    public async Task<SupportTicket?> GetByIdWithRepliesAsync(Guid id)
        => await _dbSet.AsNoTracking().Include(t => t.Replies).FirstOrDefaultAsync(t => t.Id == id);
}

public class TicketReplyRepository : GenericRepository<TicketReply>, ITicketReplyRepository
{
    public TicketReplyRepository(AppDbContext context) : base(context) { }
}

public class TravelConsultantRepository : GenericRepository<TravelConsultant>, ITravelConsultantRepository
{
    public TravelConsultantRepository(AppDbContext context) : base(context) { }
}

public class VisaAgentRepository : GenericRepository<VisaAgent>, IVisaAgentRepository
{
    public VisaAgentRepository(AppDbContext context) : base(context) { }
}

public class GuaranteeRequestRepository : GenericRepository<GuaranteeRequest>, IGuaranteeRequestRepository
{
    public GuaranteeRequestRepository(AppDbContext context) : base(context) { }

    public async Task<GuaranteeRequest?> GetByApplicationIdAsync(Guid applicationId)
        => await _dbSet.AsNoTracking().FirstOrDefaultAsync(g => g.ApplicationId == applicationId);
}

// ── New Repositories ──────────────────────────────────────────────────────────

public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Payment>> GetByApplicationIdAsync(Guid applicationId)
        => await _dbSet.AsNoTracking().Where(p => p.ApplicationId == applicationId)
                       .OrderByDescending(p => p.CreatedAt).ToListAsync();

    public async Task<IEnumerable<Payment>> GetByUserIdAsync(Guid userId)
        => await _dbSet.AsNoTracking().Where(p => p.UserId == userId)
                       .OrderByDescending(p => p.CreatedAt).ToListAsync();
}

public class AppointmentRepository : GenericRepository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Appointment>> GetByApplicationIdAsync(Guid applicationId)
        => await _dbSet.AsNoTracking().Where(a => a.ApplicationId == applicationId)
                       .OrderByDescending(a => a.ScheduledAt).ToListAsync();

    public async Task<IEnumerable<Appointment>> GetByUserIdAsync(Guid userId)
        => await _dbSet.AsNoTracking().Where(a => a.UserId == userId)
                       .OrderByDescending(a => a.ScheduledAt).ToListAsync();
}

public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(AppDbContext context) : base(context) { }

    /// <summary>
    /// Applies optional filters (userId, email, action, entity, date range)
    /// then paginates the result for compliance queries.
    /// </summary>
    public async Task<IEnumerable<AuditLog>> GetFilteredAsync(AuditLogFilterDto filter)
    {
        var query = _dbSet.AsQueryable();

        if (filter.UserId.HasValue)
            query = query.Where(l => l.UserId == filter.UserId.Value);

        if (!string.IsNullOrWhiteSpace(filter.UserEmail))
            query = query.Where(l => l.UserEmail.Contains(filter.UserEmail));

        if (!string.IsNullOrWhiteSpace(filter.Action))
            query = query.Where(l => l.Action == filter.Action);

        if (!string.IsNullOrWhiteSpace(filter.EntityName))
            query = query.Where(l => l.EntityName == filter.EntityName);

        if (filter.From.HasValue)
            query = query.Where(l => l.CreatedAt >= filter.From.Value);

        if (filter.To.HasValue)
            query = query.Where(l => l.CreatedAt <= filter.To.Value);

        var page     = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 200);

        return await query.AsNoTracking()
                          .OrderByDescending(l => l.CreatedAt)
                          .Skip((page - 1) * pageSize)
                          .Take(pageSize)
                          .ToListAsync();
    }
}

public class SettingRepository : GenericRepository<SystemSetting>, ISettingRepository
{
    public SettingRepository(AppDbContext context) : base(context) { }

    public async Task<SystemSetting?> GetByKeyAsync(string key)
        => await _dbSet.FirstOrDefaultAsync(s => s.Key == key);
}
