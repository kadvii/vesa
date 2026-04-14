using eVisaPlatform.Application.Configuration;
using eVisaPlatform.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace eVisaPlatform.Infrastructure.Services;

/// <summary>
/// MailKit-based implementation of IEmailService.
/// Sends HTML emails over SMTP using TLS/STARTTLS.
///
/// Resilience strategy:
///   - All failures are caught and logged — a failed email NEVER rolls back the
///     business transaction (the DB change is already committed).
///   - For production, consider wrapping with Polly retry or a background queue.
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly SmtpSettings          _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(
        IOptions<SmtpSettings>          settings,
        ILogger<SmtpEmailService>       logger)
    {
        _settings = settings.Value;
        _logger   = logger;
    }

    public async Task SendAsync(
        string toAddress,
        string toName,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        // Guard against misconfigured sender — avoids obscure MailKit errors
        if (string.IsNullOrWhiteSpace(_settings.FromAddress))
        {
            _logger.LogWarning("Email NOT sent: SmtpSettings.FromAddress is empty. Configure it in appsettings or User Secrets.");
            return;
        }

        try
        {
            // ── Build MIME message ────────────────────────────────────────────
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
            message.To.Add(new MailboxAddress(toName, toAddress));
            message.Subject = subject;

            // Provide plain-text alternative for clients that block HTML
            var builder = new BodyBuilder
            {
                HtmlBody  = htmlBody,
                TextBody  = StripHtml(htmlBody)  // simple fallback
            };
            message.Body = builder.ToMessageBody();

            // ── Send via MailKit ──────────────────────────────────────────────
            using var client = new SmtpClient();

            // Connect — prefer STARTTLS (port 587) over SSL (port 465)
            var secureSocketOptions = _settings.UseSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

            await client.ConnectAsync(
                _settings.Host,
                _settings.Port,
                secureSocketOptions,
                cancellationToken);

            // Authenticate only when credentials are supplied
            if (!string.IsNullOrWhiteSpace(_settings.Username))
                await client.AuthenticateAsync(
                    _settings.Username,
                    _settings.Password,
                    cancellationToken);

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(quit: true, cancellationToken);

            _logger.LogInformation(
                "Email sent successfully to {Recipient} — Subject: {Subject}",
                toAddress, subject);
        }
        catch (Exception ex)
        {
            // Log but do NOT re-throw — email failure must never abort a visa status update
            _logger.LogError(ex,
                "Failed to send email to {Recipient} — Subject: {Subject}. " +
                "The visa status was still updated. Check SMTP configuration.",
                toAddress, subject);
        }
    }

    /// <summary>Very light-weight HTML stripper for the plain-text fallback.</summary>
    private static string StripHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;
        // Remove tags, collapse whitespace
        var text = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", " ");
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\s{2,}", " ");
        return text.Trim();
    }
}
