using eVisaPlatform.Application.DTOs.Payment;

namespace eVisaPlatform.Application.Interfaces;

public interface IPaymentService
{
    /// <summary>
    /// Phase 1 — Create a Pending payment record and return a checkout session token.
    /// The frontend shows a checkout modal using this token.
    /// </summary>
    Task<CheckoutSessionDto> CreateCheckoutSessionAsync(Guid userId, CreatePaymentDto dto);

    /// <summary>
    /// Phase 2 — Called by the webhook endpoint when the gateway confirms payment.
    /// Validates the SessionToken, marks the Payment as Paid, and sets VisaStatus to Paid.
    /// </summary>
    Task<PaymentResponseDto> ConfirmPaymentAsync(WebhookCallbackDto dto);

    /// <summary>Get payment details and current status by payment ID.</summary>
    Task<PaymentResponseDto> GetByIdAsync(Guid id);

    /// <summary>Get all payments for a specific visa application.</summary>
    Task<IEnumerable<PaymentResponseDto>> GetByApplicationIdAsync(Guid applicationId);
}
