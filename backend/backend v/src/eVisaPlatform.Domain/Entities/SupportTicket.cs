using eVisaPlatform.Domain.Enums;

namespace eVisaPlatform.Domain.Entities;

public class SupportTicket
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public string? AssignedTo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public ICollection<TicketReply> Replies { get; set; } = new List<TicketReply>();
}
