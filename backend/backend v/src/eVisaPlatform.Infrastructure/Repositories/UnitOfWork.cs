using eVisaPlatform.Application.Interfaces;
using eVisaPlatform.Infrastructure.Data;
using eVisaPlatform.Infrastructure.Repositories;

namespace eVisaPlatform.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private bool _disposed;

    public IUserRepository Users { get; }
    public IVisaApplicationRepository VisaApplications { get; }
    public IDocumentRepository Documents { get; }
    public IRefreshTokenRepository RefreshTokens { get; }
    public INotificationRepository Notifications { get; }
    public IFamilyMemberRepository FamilyMembers { get; }
    public ISupportTicketRepository SupportTickets { get; }
    public ITicketReplyRepository TicketReplies { get; }
    public ITravelConsultantRepository TravelConsultants { get; }
    public IVisaAgentRepository VisaAgents { get; }
    public IGuaranteeRequestRepository GuaranteeRequests { get; }
    public IPaymentRepository Payments { get; }
    public IAppointmentRepository Appointments { get; }
    public IAuditLogRepository AuditLogs { get; }
    public ISettingRepository Settings { get; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Users = new UserRepository(context);
        VisaApplications = new VisaApplicationRepository(context);
        Documents = new DocumentRepository(context);
        RefreshTokens = new RefreshTokenRepository(context);
        Notifications = new NotificationRepository(context);
        FamilyMembers = new FamilyMemberRepository(context);
        SupportTickets = new SupportTicketRepository(context);
        TicketReplies = new TicketReplyRepository(context);
        TravelConsultants = new TravelConsultantRepository(context);
        VisaAgents = new VisaAgentRepository(context);
        GuaranteeRequests = new GuaranteeRequestRepository(context);
        Payments = new PaymentRepository(context);
        Appointments = new AppointmentRepository(context);
        AuditLogs = new AuditLogRepository(context);
        Settings = new SettingRepository(context);
    }

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    public void Dispose()
    {
        if (!_disposed)
        {
            _context.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
