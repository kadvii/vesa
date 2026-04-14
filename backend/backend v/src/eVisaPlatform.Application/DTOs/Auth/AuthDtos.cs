using System.ComponentModel.DataAnnotations;

namespace eVisaPlatform.Application.DTOs.Auth;

// ── Inbound DTOs ─────────────────────────────────────────────────────────────

public class RegisterDto
{
    [Required] public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, MinLength(8)] public string Password { get; set; } = string.Empty;
    [Required] public string ConfirmPassword { get; set; } = string.Empty;
}

public class LoginDto
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}

public class RefreshTokenDto
{
    [Required] public string RefreshToken { get; set; } = string.Empty;
}

public class LogoutDto
{
    [Required] public string RefreshToken { get; set; } = string.Empty;
}

// ── Outbound DTOs ─────────────────────────────────────────────────────────────

/// <summary>
/// Single unified response for Register, Login, and Refresh endpoints.
/// Includes Role so the frontend can apply role-based UI.
/// </summary>
public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;   // ← was missing from RegisterResponseDto
}

// Keep old names as aliases so no other file needs to change
[Obsolete("Use AuthResponseDto instead.")]
public class LoginResponseDto : AuthResponseDto { }

[Obsolete("Use AuthResponseDto instead.")]
public class RegisterResponseDto : AuthResponseDto { }
