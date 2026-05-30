using RealSync.Core.Enums;

namespace RealSync.Core.Interfaces;

/// <summary>
/// Service quản lý thông báo cho user.
/// </summary>
public interface INotificationService
{
    Task SendAsync(Guid userId, string title, string message, NotificationType type, string? link = null, object? data = null);
    Task MarkAsReadAsync(Guid notificationId);
    Task MarkAllAsReadAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
}
