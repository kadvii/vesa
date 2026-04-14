using AutoMapper;
using eVisaPlatform.Application.Common;
using eVisaPlatform.Application.DTOs.Notification;
using eVisaPlatform.Application.Interfaces;

namespace eVisaPlatform.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public NotificationService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<IEnumerable<NotificationResponseDto>>> GetUserNotificationsAsync(Guid userId)
    {
        var notifications = await _unitOfWork.Notifications.GetByUserIdAsync(userId);
        return ApiResponse<IEnumerable<NotificationResponseDto>>.Ok(_mapper.Map<IEnumerable<NotificationResponseDto>>(notifications));
    }

    public async Task<ApiResponse> MarkAsReadAsync(Guid id, Guid userId)
    {
        var notification = await _unitOfWork.Notifications.GetByIdAsync(id);
        if (notification == null || notification.UserId != userId)
            return ApiResponse.Fail("Notification not found.");

        notification.IsRead = true;
        _unitOfWork.Notifications.Update(notification);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse.Ok("Notification marked as read.");
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, Guid userId)
    {
        var notification = await _unitOfWork.Notifications.GetByIdAsync(id);
        if (notification == null || notification.UserId != userId)
            return ApiResponse.Fail("Notification not found.");

        _unitOfWork.Notifications.Delete(notification);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse.Ok("Notification deleted.");
    }
}
