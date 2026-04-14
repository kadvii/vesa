using eVisaPlatform.Application.DTOs.Auth;
using eVisaPlatform.Application.Common;

namespace eVisaPlatform.Application.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto dto);
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto);
    Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenDto dto);
    Task<ApiResponse> LogoutAsync(LogoutDto dto);
}
