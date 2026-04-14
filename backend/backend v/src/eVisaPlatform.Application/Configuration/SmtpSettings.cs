namespace eVisaPlatform.Application.Configuration;

/// <summary>
/// Strongly-typed binding for the "SmtpSettings" section in appsettings.json.
/// Sensitive values (Password) must be provided via User Secrets or environment variables
/// in non-development environments — never committed to source control.
/// </summary>
public class SmtpSettings
{
    public string Host        { get; init; } = "smtp.gmail.com";
    public int    Port        { get; init; } = 587;
    public bool   UseSsl     { get; init; } = true;
    public string Username    { get; init; } = string.Empty;
    public string Password    { get; init; } = string.Empty;
    public string FromName    { get; init; } = "Visa Z";
    public string FromAddress { get; init; } = string.Empty;
}
