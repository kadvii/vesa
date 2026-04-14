using eVisaPlatform.Application.Common;
using eVisaPlatform.Application.DTOs.Auth;
using eVisaPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace eVisaPlatform.API.Controllers;

/// <summary>
/// Handles all authentication operations: register, login, token refresh, and logout.
/// No [Authorize] attribute — all endpoints are public.
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
        => _authService = authService;

    /// <summary>Register a new user account.</summary>
    /// <remarks>Creates the user, hashes the password with BCrypt, and returns JWT tokens immediately.</remarks>
    /// <response code="201">Registration successful — returns JWT access token and role</response>
    /// <response code="400">Validation failed or email already exists</response>
    [EnableRateLimiting("Auth")]
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.RegisterAsync(dto);
        return result.Success
            ? StatusCode(StatusCodes.Status201Created, result)
            : BadRequest(result);
    }

    /// <summary>Authenticate with email and password.</summary>
    /// <response code="200">Authentication successful — returns JWT access token and role</response>
    /// <response code="401">Invalid credentials</response>
    [EnableRateLimiting("Auth")]
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.LoginAsync(dto);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    /// <summary>Refresh the access token using a valid refresh token.</summary>
    /// <response code="200">New token pair issued</response>
    /// <response code="401">Invalid or expired refresh token</response>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
    {
        var result = await _authService.RefreshTokenAsync(dto);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    /// <summary>Logout and revoke the provided refresh token.</summary>
    /// <response code="200">Logged out successfully</response>
    /// <response code="400">Refresh token not found</response>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout([FromBody] LogoutDto dto)
    {
        var result = await _authService.LogoutAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
