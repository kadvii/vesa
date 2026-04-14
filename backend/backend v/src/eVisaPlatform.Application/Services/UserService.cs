using AutoMapper;
using eVisaPlatform.Application.Common;
using eVisaPlatform.Application.DTOs.User;
using eVisaPlatform.Application.Interfaces;
using eVisaPlatform.Domain.Entities;

namespace eVisaPlatform.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPasswordService _passwordService;

    public UserService(IUnitOfWork unitOfWork, IMapper mapper, IPasswordService passwordService)
    {
        _unitOfWork      = unitOfWork;
        _mapper          = mapper;
        _passwordService = passwordService;
    }

    public async Task<ApiResponse<IEnumerable<UserResponseDto>>> GetAllUsersAsync()
    {
        var users = await _unitOfWork.Users.GetAllAsync();
        return ApiResponse<IEnumerable<UserResponseDto>>.Ok(
            _mapper.Map<IEnumerable<UserResponseDto>>(users));
    }

    public async Task<ApiResponse<UserResponseDto>> GetUserByIdAsync(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
            return ApiResponse<UserResponseDto>.Fail("User not found.");
        return ApiResponse<UserResponseDto>.Ok(_mapper.Map<UserResponseDto>(user));
    }

    public async Task<ApiResponse<UserResponseDto>> CreateUserAsync(CreateUserDto dto)
    {
        if (await _unitOfWork.Users.EmailExistsAsync(dto.Email))
            return ApiResponse<UserResponseDto>.Fail("Email is already in use.");

        var user = new User
        {
            Id           = Guid.NewGuid(),
            FullName     = dto.FullName.Trim(),
            Email        = dto.Email.Trim().ToLowerInvariant(),
            Role         = dto.Role,
            CreatedAt    = DateTime.UtcNow,
            PasswordHash = _passwordService.HashPassword(dto.Password)
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<UserResponseDto>.Ok(
            _mapper.Map<UserResponseDto>(user), "User created successfully.");
    }

    public async Task<ApiResponse> DeleteUserAsync(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
            return ApiResponse.Fail("User not found.");

        _unitOfWork.Users.Delete(user);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse.Ok("User deleted successfully.");
    }
}
