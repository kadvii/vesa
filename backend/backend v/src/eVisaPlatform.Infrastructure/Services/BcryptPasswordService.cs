using eVisaPlatform.Application.Interfaces;

namespace eVisaPlatform.Infrastructure.Services;

/// <summary>
/// BCrypt-based password hashing service.
/// Work factor of 12 is OWASP-recommended for production (2024).
/// </summary>
public class BcryptPasswordService : IPasswordService
{
    private const int WorkFactor = 12;

    public string HashPassword(string plainPassword)
    {
        return BCrypt.Net.BCrypt.HashPassword(plainPassword, WorkFactor);
    }

    public bool VerifyPassword(string plainPassword, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
    }
}
