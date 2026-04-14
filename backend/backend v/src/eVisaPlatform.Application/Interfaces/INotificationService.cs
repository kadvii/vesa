using eVisaPlatform.Application.DTOs.Notification;
using eVisaPlatform.Application.Common;

namespace eVisaPlatform.Application.Interfaces;

public interface INotificationService
{
    Task<ApiResponse<IEnumerable<NotificationResponseDto>>> GetUserNotificationsAsync(Guid userId);
    Task<ApiResponse> MarkAsReadAsync(Guid id, Guid userId);
    Task<ApiResponse> DeleteAsync(Guid id, Guid userId);
}
