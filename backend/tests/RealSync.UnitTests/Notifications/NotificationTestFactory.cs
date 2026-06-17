using Microsoft.EntityFrameworkCore;
using Moq;
using RealSync.Core.Entities;
using RealSync.Core.Enums;
using RealSync.Core.Interfaces;
using RealSync.Data.Context;
using RealSync.Services.Implementations;

namespace RealSync.UnitTests.Notifications;

internal static class NotificationTestFactory
{
    public static RealSyncDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RealSyncDbContext>()
            .UseInMemoryDatabase($"realsync-notifications-{Guid.NewGuid():N}")
            .Options;

        return new RealSyncDbContext(options);
    }

    public static NotificationService CreateService(RealSyncDbContext context)
    {
        return new NotificationService(context);
    }

    public static Mock<ICurrentUserService> CurrentUser(Guid? userId)
    {
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(x => x.UserId).Returns(userId);
        currentUser.SetupGet(x => x.IsAuthenticated).Returns(userId.HasValue);
        return currentUser;
    }

    public static User User(Guid? id = null)
    {
        return new User
        {
            Id = id ?? Guid.NewGuid(),
            FullName = "Notification User",
            Email = $"notification-{Guid.NewGuid():N}@realsync.vn",
            PasswordHash = "hash",
            RoleId = Guid.NewGuid(),
            IsActive = true
        };
    }

    public static Notification Notification(
        Guid userId,
        string title,
        NotificationType type = NotificationType.System,
        bool isRead = false,
        DateTime? createdAt = null)
    {
        return new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Message = $"{title} message",
            Type = type,
            IsRead = isRead,
            ReadAt = isRead ? DateTime.UtcNow.AddMinutes(-5) : null,
            Data = $"{{\"keyword\":\"{title}\"}}",
            Link = "/notifications",
            CreatedAt = createdAt ?? DateTime.UtcNow
        };
    }
}
