using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RealSync.Core.Entities;
using RealSync.Core.Enums;
using RealSync.Core.Interfaces;
using RealSync.Data.Context;
using RealSync.Shared.DTOs.Requests.Notifications;
using RealSync.Shared.DTOs.Responses.Notifications;
using RealSync.Shared.Exceptions;
using ValidationException = RealSync.Shared.Exceptions.ValidationException;

namespace RealSync.Services.Implementations;

/// <summary>
/// Service tạo và quản lý thông báo cho user.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly RealSyncDbContext _context;

    public NotificationService(RealSyncDbContext context)
    {
        _context = context;
    }

    public async Task SendAsync(
        Guid userId,
        string title,
        string message,
        NotificationType type,
        string? link = null,
        object? data = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            Link = link,
            Data = data != null ? JsonSerializer.Serialize(data) : null,
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    public async Task MarkAsReadAsync(Guid notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId)
            ?? throw new NotFoundException("Notification", notificationId);

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task MarkAsReadAsync(Guid userId, Guid notificationId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId)
            ?? throw new NotFoundException("Notification", notificationId);

        if (notification.IsRead)
            return;

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        var unread = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in unread)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task<(IReadOnlyList<NotificationDto> Items, int TotalCount)> GetMyNotificationsAsync(
        Guid userId,
        NotificationQueryDto query)
    {
        var notifications = BuildMyNotificationsQuery(userId, query);

        var totalCount = await notifications.CountAsync();
        notifications = ApplySorting(notifications, query.SortBy, query.SortDirection);

        var items = await notifications
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type.ToString(),
                IsRead = n.IsRead,
                ReadAt = n.ReadAt,
                Data = n.Data,
                Link = n.Link,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<NotificationDto> GetMyNotificationByIdAsync(Guid userId, Guid notificationId)
    {
        var notification = await _context.Notifications
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId)
            ?? throw new NotFoundException("Notification", notificationId);

        return MapDto(notification);
    }

    public async Task<NotificationSummaryDto> GetMySummaryAsync(Guid userId)
    {
        var totalCount = await _context.Notifications.CountAsync(n => n.UserId == userId);
        var unreadCount = await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

        return new NotificationSummaryDto
        {
            TotalCount = totalCount,
            UnreadCount = unreadCount,
            ReadCount = totalCount - unreadCount
        };
    }

    public async Task DeleteAsync(Guid userId, Guid notificationId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId)
            ?? throw new NotFoundException("Notification", notificationId);

        notification.IsDeleted = true;
        notification.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    private IQueryable<Notification> BuildMyNotificationsQuery(Guid userId, NotificationQueryDto query)
    {
        IQueryable<Notification> notifications = _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var keyword = query.Search.Trim();
            notifications = notifications.Where(n =>
                n.Title.Contains(keyword) ||
                n.Message.Contains(keyword) ||
                (n.Data != null && n.Data.Contains(keyword)));
        }

        if (query.IsRead.HasValue)
            notifications = notifications.Where(n => n.IsRead == query.IsRead.Value);

        if (!string.IsNullOrWhiteSpace(query.Type))
        {
            if (!Enum.TryParse<NotificationType>(query.Type.Trim(), true, out var notificationType))
                throw new ValidationException("type", "Loại thông báo không hợp lệ.");

            notifications = notifications.Where(n => n.Type == notificationType);
        }

        if (query.FromDate.HasValue)
            notifications = notifications.Where(n => n.CreatedAt >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            notifications = notifications.Where(n => n.CreatedAt <= query.ToDate.Value);

        return notifications;
    }

    private static IQueryable<Notification> ApplySorting(
        IQueryable<Notification> query,
        string? sortBy,
        string? sortDirection)
    {
        var ascending = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);

        return sortBy?.ToLowerInvariant() switch
        {
            "readat" or "readAt" => ascending ? query.OrderBy(n => n.ReadAt) : query.OrderByDescending(n => n.ReadAt),
            "type" => ascending ? query.OrderBy(n => n.Type) : query.OrderByDescending(n => n.Type),
            "title" => ascending ? query.OrderBy(n => n.Title) : query.OrderByDescending(n => n.Title),
            "createdat" or "createdAt" => ascending ? query.OrderBy(n => n.CreatedAt) : query.OrderByDescending(n => n.CreatedAt),
            _ => query.OrderByDescending(n => n.CreatedAt)
        };
    }

    private static NotificationDto MapDto(Notification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type.ToString(),
            IsRead = notification.IsRead,
            ReadAt = notification.ReadAt,
            Data = notification.Data,
            Link = notification.Link,
            CreatedAt = notification.CreatedAt
        };
    }
}
