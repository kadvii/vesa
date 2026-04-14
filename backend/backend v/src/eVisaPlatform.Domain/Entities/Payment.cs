using eVisaPlatform.Domain.Enums;

namespace eVisaPlatform.Domain.Entities;

/// <summary>Represents a visa fee payment tied to a VisaApplication.</summary>
public class Payment
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public Guid UserId { get; set; }

    /// <summary>Amount in the smallest currency unit (e.g., cents).</summary>
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public PaymentMethod Method { get; set; }

    /// <summary>External transaction reference from payment gateway.</summary>
    public string? TransactionReference { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }

    // Navigation
    public VisaApplication Application { get; set; } = null!;
    public User User { get; set; } = null!;
}
