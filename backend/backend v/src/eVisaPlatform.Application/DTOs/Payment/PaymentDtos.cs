using eVisaPlatform.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace eVisaPlatform.Application.DTOs.Payment;

/// <summary>Input for initiating a visa fee payment checkout session.</summary>
public class CreatePaymentDto
{
    [Required] public Guid ApplicationId { get; set; }

    [Required, Range(0.01, 100000)]
    public decimal Amount { get; set; }

    [MaxLength(3)] public string Currency { get; set; } = "USD";

    [Required] public PaymentMethod Method { get; set; }

    [MaxLength(500)] public string? Notes { get; set; }
}

/// <summary>
/// Returned after initiating a checkout session.
/// The client uses PaymentId to poll status and SessionToken to display the checkout UI.
/// </summary>
public class CheckoutSessionDto
{
    /// <summary>The newly created Payment record ID — used to poll /api/payments/{id}.</summary>
    public Guid PaymentId { get; set; }

    /// <summary>Short-lived opaque token the frontend passes to the checkout modal.</summary>
    public string SessionToken { get; set; } = string.Empty;

    /// <summary>Amount the user must pay.</summary>
    public decimal Amount { get; set; }

    /// <summary>Currency code (e.g. "USD").</summary>
    public string Currency { get; set; } = "USD";

    /// <summary>Human-readable description shown in the modal.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>ISO-8601 timestamp — token expires after 15 minutes.</summary>
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// Body sent to POST /api/payments/webhook/confirm.
/// In production this comes from the payment gateway (Stripe, PayTabs, etc.).
/// The SessionToken acts as an HMAC-style shared secret to authenticate the call.
/// </summary>
public class WebhookCallbackDto
{
    [Required] public Guid PaymentId { get; set; }

    /// <summary>
    /// Token originally issued by CreateCheckoutSessionAsync.
    /// The server validates it to ensure the callback is genuine.
    /// </summary>
    [Required] public string SessionToken { get; set; } = string.Empty;

    /// <summary>"Paid" or "Failed".</summary>
    [Required] public string Status { get; set; } = string.Empty;

    /// <summary>Reference ID returned by the external gateway (e.g. Stripe charge ID).</summary>
    public string? GatewayReference { get; set; }
}

/// <summary>API response shape for payment details.</summary>
public class PaymentResponseDto
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string? TransactionReference { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
}
