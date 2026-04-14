using eVisaPlatform.Application.DTOs.Payment;
using eVisaPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace eVisaPlatform.API.Controllers;

/// <summary>
/// Payment lifecycle for visa applications.
/// Route: /api/payments
///
/// Flow:
///   1. POST /api/payments/checkout  → returns CheckoutSessionDto (PaymentId + SessionToken)
///   2. Frontend shows modal, user completes payment
///   3. POST /api/payments/webhook/confirm → gateway posts result; server marks Paid / Failed
///   4. GET  /api/payments/{id}      → frontend polls to verify final status
/// </summary>
[ApiController]
[Route("api/payments")]
[Produces("application/json")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    // ── JWT helpers ───────────────────────────────────────────────────────────
    private Guid CurrentUserId
    {
        get
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                      ?? User.FindFirstValue("sub");
            if (!Guid.TryParse(raw, out var id) || id == Guid.Empty)
                throw new UnauthorizedAccessException(
                    "Missing or invalid user identifier in the access token.");
            return id;
        }
    }

    // ── Phase 1: Initiate Checkout ────────────────────────────────────────────
    /// <summary>
    /// POST /api/payments/checkout
    /// Creates a Pending payment and returns a short-lived checkout session.
    /// The frontend uses the returned SessionToken + PaymentId to drive the modal.
    /// </summary>
    [HttpPost("checkout")]
    [Authorize]
    public async Task<IActionResult> CreateCheckout([FromBody] CreatePaymentDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var session = await _paymentService.CreateCheckoutSessionAsync(CurrentUserId, dto);
        return Ok(session);
    }

    // ── Phase 2: Webhook Confirmation ─────────────────────────────────────────
    /// <summary>
    /// POST /api/payments/webhook/confirm
    ///
    /// In production this endpoint is called by the payment gateway (Stripe, PayTabs, etc.)
    /// NOT by the user's browser. The SessionToken authenticates the call.
    ///
    /// For development/demo purposes the frontend calls this directly after the
    /// simulated checkout modal is "completed".
    ///
    /// This endpoint is intentionally NOT [Authorize] — the gateway has no JWT.
    /// Security is provided by the SessionToken shared secret instead.
    /// </summary>
    [HttpPost("webhook/confirm")]
    [AllowAnonymous]
    public async Task<IActionResult> WebhookConfirm([FromBody] WebhookCallbackDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _paymentService.ConfirmPaymentAsync(dto);
        return Ok(result);
    }

    // ── Read: single payment ──────────────────────────────────────────────────
    /// <summary>
    /// GET /api/payments/{id}
    /// Poll this endpoint after initiating checkout to check if the payment succeeded.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _paymentService.GetByIdAsync(id);
        return Ok(result);
    }

    // ── Read: all for application ────────────────────────────────────────────
    /// <summary>
    /// GET /api/payments/application/{applicationId}
    /// Get all payments for a specific visa application.
    /// </summary>
    [HttpGet("application/{applicationId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetByApplication(Guid applicationId)
    {
        var result = await _paymentService.GetByApplicationIdAsync(applicationId);
        return Ok(result);
    }
}
