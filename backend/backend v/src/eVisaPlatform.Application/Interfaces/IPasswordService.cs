namespace eVisaPlatform.Application.Interfaces;

/// <summary>
/// Abstraction for password hashing and verification.
/// Decouples Application layer from BCrypt implementation detail.
/// </summary>
public interface IPasswordService
{
    string HashPassword(string plainPassword);
    bool VerifyPassword(string plainPassword, string hashedPassword);
}
