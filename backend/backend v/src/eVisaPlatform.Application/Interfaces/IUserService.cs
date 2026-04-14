using eVisaPlatform.Application.DTOs.User;
using eVisaPlatform.Application.Common;

namespace eVisaPlatform.Application.Interfaces;

public interface IUserService
{
    Task<ApiResponse<IEnumerable<UserResponseDto>>> GetAllUsersAsync();
    Task<ApiResponse<UserResponseDto>> GetUserByIdAsync(Guid id);
    Task<ApiResponse<UserResponseDto>> CreateUserAsync(CreateUserDto dto);
    Task<ApiResponse> DeleteUserAsync(Guid id);
}
