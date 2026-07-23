using Current.Api.DTOs.Notifications;
using Current.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Current.Api.Controllers;

[Authorize]
[ApiController]
[Route("notifications")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ICurrentUserService _currentUserService;

    public NotificationsController(
        INotificationService notificationService,
        ICurrentUserService currentUserService)
    {
        _notificationService = notificationService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotificationResponse>>> GetAll()
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var notifications = await _notificationService.GetNotificationsAsync(currentUserId);
        return Ok(notifications);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<UnreadNotificationCountResponse>> GetUnreadCount()
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var unreadCount = await _notificationService.GetUnreadCountAsync(currentUserId);

        return Ok(new UnreadNotificationCountResponse
        {
            Count = unreadCount,
        });
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        await _notificationService.MarkAllAsReadAsync(currentUserId);
        return NoContent();
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var notificationMarked = await _notificationService.MarkAsReadAsync(id, currentUserId);

        if (!notificationMarked)
        {
            return NotFound();
        }

        return NoContent();
    }
}
