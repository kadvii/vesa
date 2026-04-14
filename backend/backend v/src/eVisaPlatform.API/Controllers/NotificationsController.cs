using eVisaPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace eVisaPlatform.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                   User.FindFirstValue("sub") ?? Guid.Empty.ToString());

    /// <summary>Get all notifications for the authenticated user</summary>
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        var result = await _notificationService.GetUserNotificationsAsync(CurrentUserId);
        return Ok(result);
    }

    /// <summary>Mark a notification as read</summary>
    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var result = await _notificationService.MarkAsReadAsync(id, CurrentUserId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Delete a notification</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _notificationService.DeleteAsync(id, CurrentUserId);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
