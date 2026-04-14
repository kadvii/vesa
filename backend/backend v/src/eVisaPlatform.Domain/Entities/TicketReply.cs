namespace eVisaPlatform.Domain.Entities;

public class TicketReply
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid SenderId { get; set; } // Can be User or Admin
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public SupportTicket Ticket { get; set; } = null!;
    public User Sender { get; set; } = null!;
}
