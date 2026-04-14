using eVisaPlatform.Application.Common;
using eVisaPlatform.Application.DTOs.Auth;
using eVisaPlatform.Application.Interfaces;
using eVisaPlatform.Domain.Entities;
using eVisaPlatform.Domain.Enums;

namespace eVisaPlatform.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordService _passwordService;
    private readonly IDateTimeProvider _clock;

    public AuthService(
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        IPasswordService passwordService,
        IDateTimeProvider clock)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _passwordService = passwordService;
        _clock = clock;
    }

    // ── Register ──────────────────────────────────────────────────────────────
    public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto dto)
    {
        // 1. Confirm passwords match
        if (dto.Password != dto.ConfirmPassword)
            return ApiResponse<AuthResponseDto>.Fail("Password and ConfirmPassword do not match.");

        // 2. Check for duplicate email
        var existing = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
        if (existing != null)
            return ApiResponse<AuthResponseDto>.Fail("This email is already registered.");

        // 3. Create user with BCrypt hash
        var user = new User
        {
            Id           = Guid.NewGuid(),
            FullName     = dto.FullName.Trim(),
            Email        = dto.Email.Trim().ToLowerInvariant(),
            Role         = UserRole.User,          // default role on self-registration
            CreatedAt    = _clock.UtcNow,
            PasswordHash = _passwordService.HashPassword(dto.Password)
        };

        await _unitOfWork.Users.AddAsync(user);

        // 4. Persist refresh token
        var (refreshTokenEntity, refreshTokenValue) = BuildRefreshToken(user.Id);
        await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
        await _unitOfWork.SaveChangesAsync();

        // 5. Return tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        return ApiResponse<AuthResponseDto>.Ok(
            BuildAuthResponse(user, accessToken, refreshTokenValue),
            "Registration successful.");
    }

    // ── Login ─────────────────────────────────────────────────────────────────
    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
        if (user == null)
            return ApiResponse<AuthResponseDto>.Fail("Invalid email or password.");

        // Constant-time BCrypt verify
        if (!_passwordService.VerifyPassword(dto.Password, user.PasswordHash))
            return ApiResponse<AuthResponseDto>.Fail("Invalid email or password.");

        // Revoke any existing refresh tokens (single-session per account)
        await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(user.Id);

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var (refreshTokenEntity, refreshTokenValue) = BuildRefreshToken(user.Id);
        await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<AuthResponseDto>.Ok(
            BuildAuthResponse(user, accessToken, refreshTokenValue),
            "Login successful.");
    }

    // ── Refresh Token ─────────────────────────────────────────────────────────
    public async Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenDto dto)
    {
        var token = await _unitOfWork.RefreshTokens.GetByTokenAsync(dto.RefreshToken);
        if (token == null || token.IsRevoked || token.ExpiresAt < DateTime.UtcNow)
            return ApiResponse<AuthResponseDto>.Fail("Invalid or expired refresh token.");

        var user = await _unitOfWork.Users.GetByIdAsync(token.UserId);
        if (user == null)
            return ApiResponse<AuthResponseDto>.Fail("User not found.");

        // Rotate: revoke old, issue new
        token.IsRevoked = true;
        _unitOfWork.RefreshTokens.Update(token);

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var (newRefreshTokenEntity, newRefreshTokenValue) = BuildRefreshToken(user.Id);
        await _unitOfWork.RefreshTokens.AddAsync(newRefreshTokenEntity);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<AuthResponseDto>.Ok(
            BuildAuthResponse(user, accessToken, newRefreshTokenValue),
            "Token refreshed.");
    }

    // ── Logout ────────────────────────────────────────────────────────────────
    public async Task<ApiResponse> LogoutAsync(LogoutDto dto)
    {
        var token = await _unitOfWork.RefreshTokens.GetByTokenAsync(dto.RefreshToken);
        if (token == null)
            return ApiResponse.Fail("Refresh token not found.");

        if (!token.IsRevoked)
        {
            token.IsRevoked = true;
            _unitOfWork.RefreshTokens.Update(token);
            await _unitOfWork.SaveChangesAsync();
        }

        return ApiResponse.Ok("Logged out successfully.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private (RefreshToken entity, string value) BuildRefreshToken(Guid userId)
    {
        var value = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
        var entity = new RefreshToken
        {
            Id        = Guid.NewGuid(),
            UserId    = userId,
            Token     = value,
            ExpiresAt = _clock.UtcNow.AddDays(7)
        };
        return (entity, value);
    }

    private AuthResponseDto BuildAuthResponse(User user, string accessToken, string refreshToken) =>
        new()
        {
            AccessToken  = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt    = _clock.UtcNow.AddHours(1),
            FullName     = user.FullName,
            Email        = user.Email,
            Role         = user.Role.ToString()
        };
}
