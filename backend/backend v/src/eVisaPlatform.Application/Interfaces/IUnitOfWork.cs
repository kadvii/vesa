namespace eVisaPlatform.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IVisaApplicationRepository VisaApplications { get; }
    IDocumentRepository Documents { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    INotificationRepository Notifications { get; }
    IFamilyMemberRepository FamilyMembers { get; }
    ISupportTicketRepository SupportTickets { get; }
    ITicketReplyRepository TicketReplies { get; }
    ITravelConsultantRepository TravelConsultants { get; }
    IVisaAgentRepository VisaAgents { get; }
    IGuaranteeRequestRepository GuaranteeRequests { get; }
    IPaymentRepository Payments { get; }
    IAppointmentRepository Appointments { get; }
    IAuditLogRepository AuditLogs { get; }
    ISettingRepository Settings { get; }

    Task<int> SaveChangesAsync();
}
