using Microsoft.AspNetCore.Mvc;
using RealSync.Api.Filters;
using RealSync.Core.Interfaces;
using RealSync.Shared.DTOs.Requests.Notifications;
using RealSync.Shared.DTOs.Responses.Notifications;

namespace RealSync.Api.Controllers;

[Route("api/notifications")]
[Route("api/v1/notifications")]
public class NotificationsController : BaseController
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
    [RequirePermission("notifications.read")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<IEnumerable<NotificationDto>>), 200)]
    public async Task<IActionResult> GetMyNotifications([FromQuery] NotificationQueryDto query)
    {
        var userId = GetCurrentUserId();
        var (items, totalCount) = await _notificationService.GetMyNotificationsAsync(userId, query);
        return PagedResponse(items, query.Page, query.PageSize, totalCount);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("notifications.read")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<NotificationDto>), 200)]
    public async Task<IActionResult> GetMyNotification(Guid id)
    {
        var result = await _notificationService.GetMyNotificationByIdAsync(GetCurrentUserId(), id);
        return OkResponse(result);
    }

    [HttpGet("unread-count")]
    [RequirePermission("notifications.read")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<object>), 200)]
    public async Task<IActionResult> GetUnreadCount()
    {
        var unreadCount = await _notificationService.GetUnreadCountAsync(GetCurrentUserId());
        return OkResponse(new { unreadCount });
    }

    [HttpGet("summary")]
    [RequirePermission("notifications.read")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<NotificationSummaryDto>), 200)]
    public async Task<IActionResult> GetSummary()
    {
        var result = await _notificationService.GetMySummaryAsync(GetCurrentUserId());
        return OkResponse(result);
    }

    [HttpPatch("{id:guid}/read")]
    [RequirePermission("notifications.update")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        await _notificationService.MarkAsReadAsync(GetCurrentUserId(), id);
        return NoContent();
    }

    [HttpPatch("read-all")]
    [RequirePermission("notifications.update")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> MarkAllAsRead()
    {
        await _notificationService.MarkAllAsReadAsync(GetCurrentUserId());
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("notifications.delete")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteNotification(Guid id)
    {
        await _notificationService.DeleteAsync(GetCurrentUserId(), id);
        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        return _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");
    }
}
