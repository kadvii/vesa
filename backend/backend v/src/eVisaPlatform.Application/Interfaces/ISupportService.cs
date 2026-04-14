using eVisaPlatform.Application.DTOs.Support;

namespace eVisaPlatform.Application.Interfaces;

public interface ISupportService
{
    Task<SupportTicketResponseDto> CreateTicketAsync(Guid userId, CreateSupportTicketDto dto);
    Task<IEnumerable<SupportTicketResponseDto>> GetUserTicketsAsync(Guid userId);
    Task<TicketReplyResponseDto> AddReplyAsync(Guid userId, Guid ticketId, CreateTicketReplyDto dto);
    Task CloseTicketAsync(Guid userId, Guid ticketId);
}
