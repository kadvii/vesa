namespace eVisaPlatform.Application.Interfaces;

/// <summary>
/// Contract for sending transactional emails.
/// The Implementation lives in Infrastructure (MailKit) so the Application
/// layer stays free of SMTP concerns.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send an HTML email to a single recipient.
    /// </summary>
    /// <param name="toAddress">Recipient email address.</param>
    /// <param name="toName">Recipient display name (used in MIME "To" header).</param>
    /// <param name="subject">Email subject line.</param>
    /// <param name="htmlBody">Full HTML body string.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task SendAsync(
        string toAddress,
        string toName,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default);
}
