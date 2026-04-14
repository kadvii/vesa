namespace eVisaPlatform.Application.DTOs.Support;

public class TicketReplyResponseDto
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid SenderId { get; set; }
    public DateTime CreatedAt { get; set; }
}
