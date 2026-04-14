using eVisaPlatform.Application.DTOs.AuditLog;
using eVisaPlatform.Domain.Entities;

namespace eVisaPlatform.Application.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
}

public interface IVisaApplicationRepository : IGenericRepository<VisaApplication>
{
    Task<IEnumerable<VisaApplication>> GetAllWithDetailsAsync();
    Task<VisaApplication?> GetByIdWithDetailsAsync(Guid id);
    Task<IEnumerable<VisaApplication>> GetByUserIdAsync(Guid userId);

    Task<(IReadOnlyList<VisaApplication> Items, int TotalCount)> GetAllWithDetailsPagedAsync(
        int page, int pageSize, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<VisaApplication> Items, int TotalCount)> GetByUserIdPagedAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
}

public interface IDocumentRepository : IGenericRepository<Document>
{
    Task<IEnumerable<Document>> GetByApplicationIdAsync(Guid applicationId);
}

public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task RevokeAllUserTokensAsync(Guid userId);
    Task<int> DeleteExpiredAndRevokedAsync(CancellationToken cancellationToken = default);
}

public interface INotificationRepository : IGenericRepository<Notification>
{
    Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId);
}

public interface IFamilyMemberRepository : IGenericRepository<FamilyMember>
{
    Task<IEnumerable<FamilyMember>> GetByApplicationIdAsync(Guid applicationId);
}

public interface ISupportTicketRepository : IGenericRepository<SupportTicket>
{
    Task<IEnumerable<SupportTicket>> GetByUserIdAsync(Guid userId);
    Task<SupportTicket?> GetByIdWithRepliesAsync(Guid id);
}

public interface ITicketReplyRepository : IGenericRepository<TicketReply>
{
}

public interface ITravelConsultantRepository : IGenericRepository<TravelConsultant>
{
}

public interface IVisaAgentRepository : IGenericRepository<VisaAgent>
{
}

public interface IGuaranteeRequestRepository : IGenericRepository<GuaranteeRequest>
{
    Task<GuaranteeRequest?> GetByApplicationIdAsync(Guid applicationId);
}

// ── New Repositories ────────────────────────────────────────────

public interface IPaymentRepository : IGenericRepository<Payment>
{
    Task<IEnumerable<Payment>> GetByApplicationIdAsync(Guid applicationId);
    Task<IEnumerable<Payment>> GetByUserIdAsync(Guid userId);
}

public interface IAppointmentRepository : IGenericRepository<Appointment>
{
    Task<IEnumerable<Appointment>> GetByApplicationIdAsync(Guid applicationId);
    Task<IEnumerable<Appointment>> GetByUserIdAsync(Guid userId);
}

public interface IAuditLogRepository : IGenericRepository<AuditLog>
{
    /// <summary>Filtered, paginated query across audit logs.</summary>
    Task<IEnumerable<AuditLog>> GetFilteredAsync(AuditLogFilterDto filter);
}

public interface ISettingRepository : IGenericRepository<SystemSetting>
{
    Task<SystemSetting?> GetByKeyAsync(string key);
}
