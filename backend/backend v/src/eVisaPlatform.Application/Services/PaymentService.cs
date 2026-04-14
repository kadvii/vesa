using AutoMapper;
using eVisaPlatform.Application.Common;
using eVisaPlatform.Application.DTOs.Payment;
using eVisaPlatform.Application.Interfaces;
using eVisaPlatform.Domain.Entities;
using eVisaPlatform.Domain.Enums;
using System.Security.Cryptography;

namespace eVisaPlatform.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork       _unitOfWork;
    private readonly IMapper           _mapper;
    private readonly IDateTimeProvider _clock;

    // Session token TTL — matches the 15-minute window shown in the frontend modal
    private static readonly TimeSpan SessionTokenTtl = TimeSpan.FromMinutes(15);

    public PaymentService(
        IUnitOfWork       unitOfWork,
        IMapper           mapper,
        IDateTimeProvider clock)
    {
        _unitOfWork = unitOfWork;
        _mapper     = mapper;
        _clock      = clock;
    }

    // ── Phase 1: Create Checkout Session ─────────────────────────────────────
    /// <summary>
    /// Creates a Pending payment record, generates a cryptographically random
    /// session token, and returns a CheckoutSessionDto for the frontend to display
    /// its checkout modal. The token is stored on the Payment entity so the
    /// webhook handler can validate it without a database round-trip bypass.
    /// </summary>
    public async Task<CheckoutSessionDto> CreateCheckoutSessionAsync(
        Guid userId, CreatePaymentDto dto)
    {
        // 1. Validate the linked application exists and belongs to the user
        var app = await _unitOfWork.VisaApplications.GetByIdAsync(dto.ApplicationId)
            ?? throw new KeyNotFoundException("Visa application not found.");

        if (app.UserId != userId)
            throw new UnauthorizedAccessException(
                "You are not authorised to pay for this application.");

        // 2. Block if already paid
        var existing = await _unitOfWork.Payments.GetByApplicationIdAsync(dto.ApplicationId);
        if (existing.Any(p => p.Status == PaymentStatus.Paid))
            throw new InvalidOperationException(
                "A successful payment already exists for this application.");

        // 3. Generate cryptographically random session token (256-bit)
        var tokenBytes    = RandomNumberGenerator.GetBytes(32);
        var sessionToken  = Convert.ToBase64String(tokenBytes);
        var expiresAt     = _clock.UtcNow.Add(SessionTokenTtl);

        // 4. Create payment in Pending state — token stored for webhook validation
        var payment = new Payment
        {
            Id                   = Guid.NewGuid(),
            ApplicationId        = dto.ApplicationId,
            UserId               = userId,
            Amount               = dto.Amount,
            Currency             = dto.Currency,
            Method               = dto.Method,
            Notes                = dto.Notes,
            Status               = PaymentStatus.Pending,
            CreatedAt            = _clock.UtcNow,
            // Reuse TransactionReference to store the session token temporarily
            // (in production you'd store it in a dedicated column or cache)
            TransactionReference = sessionToken
        };

        await _unitOfWork.Payments.AddAsync(payment);

        // 5. Mark application as PendingPayment so the UI reflects the state
        app.Status = VisaStatus.PendingPayment;
        _unitOfWork.VisaApplications.Update(app);

        await _unitOfWork.SaveChangesAsync();

        return new CheckoutSessionDto
        {
            PaymentId   = payment.Id,
            SessionToken = sessionToken,
            Amount      = dto.Amount,
            Currency    = dto.Currency,
            Description = $"رسوم تأشيرة {app.VisaType} إلى {app.DestinationCountry ?? "الوجهة المختارة"}",
            ExpiresAt   = expiresAt,
        };
    }

    // ── Phase 2: Confirm Payment (Webhook) ────────────────────────────────────
    /// <summary>
    /// Called by the payment gateway callback (or the simulated frontend confirm).
    /// Validates the session token, marks the payment as Paid or Failed,
    /// and — on success — transitions the visa application to Paid.
    /// </summary>
    public async Task<PaymentResponseDto> ConfirmPaymentAsync(WebhookCallbackDto dto)
    {
        var payment = await _unitOfWork.Payments.GetByIdAsync(dto.PaymentId)
            ?? throw new KeyNotFoundException("Payment not found.");

        // 1. Validate token — prevents forgery
        if (payment.TransactionReference != dto.SessionToken)
            throw new UnauthorizedAccessException(
                "Invalid session token. Payment confirmation rejected.");

        // 2. Check token expiry (stored indirectly via CreatedAt + TTL)
        if (_clock.UtcNow > payment.CreatedAt.Add(SessionTokenTtl))
            throw new InvalidOperationException(
                "Checkout session has expired. Please initiate a new payment.");

        // 3. Idempotency — never re-process
        if (payment.Status == PaymentStatus.Paid)
            return _mapper.Map<PaymentResponseDto>(payment);

        var status = dto.Status.Trim();

        if (status == "Paid")
        {
            payment.Status               = PaymentStatus.Paid;
            payment.PaidAt               = _clock.UtcNow;
            // Replace the session token with the real gateway reference
            payment.TransactionReference = dto.GatewayReference
                                           ?? $"TXN-{Guid.NewGuid():N}".ToUpper()[..20];

            // Transition visa application to Paid
            var app = await _unitOfWork.VisaApplications.GetByIdAsync(payment.ApplicationId);
            if (app is not null)
            {
                app.Status = VisaStatus.Paid;
                _unitOfWork.VisaApplications.Update(app);
            }
        }
        else if (status == "Failed")
        {
            payment.Status               = PaymentStatus.Failed;
            payment.TransactionReference = dto.GatewayReference ?? payment.TransactionReference;
        }
        else
        {
            throw new ArgumentException($"Unknown payment status '{status}'. Expected 'Paid' or 'Failed'.");
        }

        _unitOfWork.Payments.Update(payment);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<PaymentResponseDto>(payment);
    }

    // ── Read ──────────────────────────────────────────────────────────────────
    public async Task<PaymentResponseDto> GetByIdAsync(Guid id)
    {
        var payment = await _unitOfWork.Payments.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Payment not found.");
        return _mapper.Map<PaymentResponseDto>(payment);
    }

    public async Task<IEnumerable<PaymentResponseDto>> GetByApplicationIdAsync(Guid applicationId)
    {
        var payments = await _unitOfWork.Payments.GetByApplicationIdAsync(applicationId);
        return _mapper.Map<IEnumerable<PaymentResponseDto>>(payments);
    }
}
