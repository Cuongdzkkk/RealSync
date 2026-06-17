using RealSync.Core.Enums;
using RealSync.Shared.DTOs.Requests.Notifications;
using RealSync.Shared.DTOs.Responses.Notifications;

namespace RealSync.Core.Interfaces;

/// <summary>
/// Service quản lý thông báo cho user.
/// </summary>
public interface INotificationService
{
    Task SendAsync(Guid userId, string title, string message, NotificationType type, string? link = null, object? data = null);
    Task MarkAsReadAsync(Guid notificationId);
    Task MarkAsReadAsync(Guid userId, Guid notificationId);
    Task MarkAllAsReadAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task<(IReadOnlyList<NotificationDto> Items, int TotalCount)> GetMyNotificationsAsync(Guid userId, NotificationQueryDto query);
    Task<NotificationDto> GetMyNotificationByIdAsync(Guid userId, Guid notificationId);
    Task<NotificationSummaryDto> GetMySummaryAsync(Guid userId);
    Task DeleteAsync(Guid userId, Guid notificationId);
}
