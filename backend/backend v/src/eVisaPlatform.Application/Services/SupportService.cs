using AutoMapper;
using eVisaPlatform.Application.Common;
using eVisaPlatform.Application.DTOs.Support;
using eVisaPlatform.Application.Interfaces;
using eVisaPlatform.Domain.Entities;
using eVisaPlatform.Domain.Enums;

namespace eVisaPlatform.Application.Services;

public class SupportService : ISupportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SupportService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SupportTicketResponseDto> CreateTicketAsync(Guid userId, CreateSupportTicketDto dto)
    {
        var ticket = new SupportTicket
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = dto.Title,
            Description = dto.Description,
            Status = TicketStatus.Open,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.SupportTickets.AddAsync(ticket);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<SupportTicketResponseDto>(ticket);
    }

    public async Task<IEnumerable<SupportTicketResponseDto>> GetUserTicketsAsync(Guid userId)
    {
        var tickets = await _unitOfWork.SupportTickets.GetByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<SupportTicketResponseDto>>(tickets);
    }

    public async Task<TicketReplyResponseDto> AddReplyAsync(Guid userId, Guid ticketId, CreateTicketReplyDto dto)
    {
        var ticket = await _unitOfWork.SupportTickets.GetByIdAsync(ticketId)
            ?? throw new KeyNotFoundException("Support ticket not found.");
        if (ticket.UserId != userId && !await IsUserAdmin(userId))
            throw new UnauthorizedAccessException("You are not authorized to reply to this ticket.");

        var reply = new TicketReply
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            Message = dto.Message,
            SenderId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.TicketReplies.AddAsync(reply);
        
        if (ticket.Status == TicketStatus.Closed)
            ticket.Status = TicketStatus.Open;

        _unitOfWork.SupportTickets.Update(ticket);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<TicketReplyResponseDto>(reply);
    }

    public async Task CloseTicketAsync(Guid userId, Guid ticketId)
    {
        var ticket = await _unitOfWork.SupportTickets.GetByIdAsync(ticketId)
            ?? throw new KeyNotFoundException("Support ticket not found.");
        if (ticket.UserId != userId && !await IsUserAdmin(userId))
            throw new UnauthorizedAccessException("You are not authorized to close this ticket.");

        ticket.Status = TicketStatus.Closed;
        _unitOfWork.SupportTickets.Update(ticket);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<bool> IsUserAdmin(Guid userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        return user != null && user.Role == UserRole.Admin;
    }
}
