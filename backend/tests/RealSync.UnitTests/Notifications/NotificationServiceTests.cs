using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RealSync.Core.Enums;
using RealSync.Shared.DTOs.Requests.Notifications;
using RealSync.Shared.Exceptions;

namespace RealSync.UnitTests.Notifications;

[TestFixture]
public class NotificationServiceTests
{
    [Test]
    public async Task GetMyNotificationsAsync_ShouldReturnOnlyCurrentUserNotifications()
    {
        await using var context = NotificationTestFactory.CreateContext();
        var userA = NotificationTestFactory.User();
        var userB = NotificationTestFactory.User();
        context.Users.AddRange(userA, userB);
        context.Notifications.AddRange(
            NotificationTestFactory.Notification(userA.Id, "A1"),
            NotificationTestFactory.Notification(userA.Id, "A2"),
            NotificationTestFactory.Notification(userB.Id, "B1"));
        await context.SaveChangesAsync();

        var result = await NotificationTestFactory.CreateService(context)
            .GetMyNotificationsAsync(userA.Id, new NotificationQueryDto());

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items.Should().OnlyContain(n => n.Title.StartsWith("A"));
    }

    [Test]
    public async Task GetMyNotificationsAsync_ShouldApplyPagination()
    {
        await using var context = NotificationTestFactory.CreateContext();
        var user = NotificationTestFactory.User();
        context.Users.Add(user);
        context.Notifications.AddRange(
            NotificationTestFactory.Notification(user.Id, "N1"),
            NotificationTestFactory.Notification(user.Id, "N2"),
            NotificationTestFactory.Notification(user.Id, "N3"));
        await context.SaveChangesAsync();

        var result = await NotificationTestFactory.CreateService(context)
            .GetMyNotificationsAsync(user.Id, new NotificationQueryDto { Page = 2, PageSize = 2 });

        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(3);
    }

    [Test]
    public async Task GetMyNotificationsAsync_ShouldFilterByIsRead()
    {
        await using var context = NotificationTestFactory.CreateContext();
        var user = NotificationTestFactory.User();
        context.Users.Add(user);
        context.Notifications.AddRange(
            NotificationTestFactory.Notification(user.Id, "Unread", isRead: false),
            NotificationTestFactory.Notification(user.Id, "Read", isRead: true));
        await context.SaveChangesAsync();
        var service = NotificationTestFactory.CreateService(context);

        var unread = await service.GetMyNotificationsAsync(user.Id, new NotificationQueryDto { IsRead = false });
        var read = await service.GetMyNotificationsAsync(user.Id, new NotificationQueryDto { IsRead = true });

        unread.Items.Should().ContainSingle(n => !n.IsRead);
        read.Items.Should().ContainSingle(n => n.IsRead);
    }

    [Test]
    public async Task GetMyNotificationsAsync_ShouldFilterByType()
    {
        await using var context = NotificationTestFactory.CreateContext();
        var user = NotificationTestFactory.User();
        context.Users.Add(user);
        context.Notifications.AddRange(
            NotificationTestFactory.Notification(user.Id, "Lead", NotificationType.Lead),
            NotificationTestFactory.Notification(user.Id, "System", NotificationType.System));
        await context.SaveChangesAsync();

        var result = await NotificationTestFactory.CreateService(context)
            .GetMyNotificationsAsync(user.Id, new NotificationQueryDto { Type = "Lead" });

        result.Items.Should().ContainSingle(n => n.Type == "Lead");
    }

    [Test]
    public async Task GetMyNotificationsAsync_ShouldApplySearch()
    {
        await using var context = NotificationTestFactory.CreateContext();
        var user = NotificationTestFactory.User();
        context.Users.Add(user);
        context.Notifications.AddRange(
            NotificationTestFactory.Notification(user.Id, "Searchable"),
            NotificationTestFactory.Notification(user.Id, "Other"));
        await context.SaveChangesAsync();

        var result = await NotificationTestFactory.CreateService(context)
            .GetMyNotificationsAsync(user.Id, new NotificationQueryDto { Search = "Searchable" });

        result.Items.Should().ContainSingle(n => n.Title == "Searchable");
    }

    [Test]
    public async Task GetMyNotificationsAsync_ShouldOrderByCreatedAtDescByDefault()
    {
        await using var context = NotificationTestFactory.CreateContext();
        var user = NotificationTestFactory.User();
        context.Users.Add(user);
        var oldNotification = NotificationTestFactory.Notification(user.Id, "Old");
        var newNotification = NotificationTestFactory.Notification(user.Id, "New");
        context.Notifications.AddRange(oldNotification, newNotification);
        await context.SaveChangesAsync();
        oldNotification.CreatedAt = DateTime.UtcNow.AddDays(-1);
        newNotification.CreatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        var result = await NotificationTestFactory.CreateService(context)
            .GetMyNotificationsAsync(user.Id, new NotificationQueryDto());

        result.Items.First().Title.Should().Be("New");
    }

    [Test]
    public async Task GetMyNotificationByIdAsync_ShouldEnforceOwnership()
    {
        await using var context = NotificationTestFactory.CreateContext();
        var userA = NotificationTestFactory.User();
        var userB = NotificationTestFactory.User();
        var owned = NotificationTestFactory.Notification(userA.Id, "Owned");
        var other = NotificationTestFactory.Notification(userB.Id, "Other");
        context.Users.AddRange(userA, userB);
        context.Notifications.AddRange(owned, other);
        await context.SaveChangesAsync();
        var service = NotificationTestFactory.CreateService(context);

        var result = await service.GetMyNotificationByIdAsync(userA.Id, owned.Id);

        result.Id.Should().Be(owned.Id);
        await service.Invoking(s => s.GetMyNotificationByIdAsync(userA.Id, other.Id))
            .Should().ThrowAsync<NotFoundException>();
        await service.Invoking(s => s.GetMyNotificationByIdAsync(userA.Id, Guid.NewGuid()))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task GetUnreadCountAsync_ShouldCountOnlyUnreadOfUser()
    {
        await using var context = NotificationTestFactory.CreateContext();
        var userA = NotificationTestFactory.User();
        var userB = NotificationTestFactory.User();
        context.Users.AddRange(userA, userB);
        context.Notifications.AddRange(
            NotificationTestFactory.Notification(userA.Id, "Unread", isRead: false),
            NotificationTestFactory.Notification(userA.Id, "Read", isRead: true),
            NotificationTestFactory.Notification(userB.Id, "Other Unread", isRead: false));
        await context.SaveChangesAsync();

        var count = await NotificationTestFactory.CreateService(context).GetUnreadCountAsync(userA.Id);

        count.Should().Be(1);
    }

    [Test]
    public async Task GetMySummaryAsync_ShouldReturnTotalReadUnread()
    {
        await using var context = NotificationTestFactory.CreateContext();
        var user = NotificationTestFactory.User();
        context.Users.Add(user);
        context.Notifications.AddRange(
            NotificationTestFactory.Notification(user.Id, "Unread", isRead: false),
            NotificationTestFactory.Notification(user.Id, "Read", isRead: true));
        await context.SaveChangesAsync();

        var summary = await NotificationTestFactory.CreateService(context).GetMySummaryAsync(user.Id);

        summary.TotalCount.Should().Be(2);
        summary.UnreadCount.Should().Be(1);
        summary.ReadCount.Should().Be(1);
    }

    [Test]
    public async Task MarkAsReadAsync_ShouldMarkOwnedUnreadAndIgnoreAlreadyRead()
    {
        await using var context = NotificationTestFactory.CreateContext();
        var user = NotificationTestFactory.User();
        var unread = NotificationTestFactory.Notification(user.Id, "Unread");
        var read = NotificationTestFactory.Notification(user.Id, "Read", isRead: true);
        context.Users.Add(user);
        context.Notifications.AddRange(unread, read);
        await context.SaveChangesAsync();
        var service = NotificationTestFactory.CreateService(context);

        await service.MarkAsReadAsync(user.Id, unread.Id);
        await service.MarkAsReadAsync(user.Id, read.Id);

        var updated = await context.Notifications.FirstAsync(n => n.Id == unread.Id);
        updated.IsRead.Should().BeTrue();
        updated.ReadAt.Should().NotBeNull();
    }

    [Test]
    public async Task MarkAsReadAsync_OtherUserOrMissingNotification_ShouldThrowNotFoundException()
    {
        await using var context = NotificationTestFactory.CreateContext();
        var userA = NotificationTestFactory.User();
        var userB = NotificationTestFactory.User();
        var other = NotificationTestFactory.Notification(userB.Id, "Other");
        context.Users.AddRange(userA, userB);
        context.Notifications.Add(other);
        await context.SaveChangesAsync();
        var service = NotificationTestFactory.CreateService(context);

        await service.Invoking(s => s.MarkAsReadAsync(userA.Id, other.Id))
            .Should().ThrowAsync<NotFoundException>();
        await service.Invoking(s => s.MarkAsReadAsync(userA.Id, Guid.NewGuid()))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task MarkAllAsReadAsync_ShouldMarkOnlyCurrentUserUnreadNotifications()
    {
        await using var context = NotificationTestFactory.CreateContext();
        var userA = NotificationTestFactory.User();
        var userB = NotificationTestFactory.User();
        context.Users.AddRange(userA, userB);
        context.Notifications.AddRange(
            NotificationTestFactory.Notification(userA.Id, "A1"),
            NotificationTestFactory.Notification(userA.Id, "A2"),
            NotificationTestFactory.Notification(userB.Id, "B1"));
        await context.SaveChangesAsync();

        await NotificationTestFactory.CreateService(context).MarkAllAsReadAsync(userA.Id);

        (await context.Notifications.Where(n => n.UserId == userA.Id).ToListAsync()).Should().OnlyContain(n => n.IsRead);
        (await context.Notifications.FirstAsync(n => n.UserId == userB.Id)).IsRead.Should().BeFalse();
    }

    [Test]
    public async Task DeleteAsync_ShouldSoftDeleteOwnedNotification()
    {
        await using var context = NotificationTestFactory.CreateContext();
        var user = NotificationTestFactory.User();
        var notification = NotificationTestFactory.Notification(user.Id, "Delete");
        context.Users.Add(user);
        context.Notifications.Add(notification);
        await context.SaveChangesAsync();

        await NotificationTestFactory.CreateService(context).DeleteAsync(user.Id, notification.Id);

        var deleted = await context.Notifications.IgnoreQueryFilters().FirstAsync(n => n.Id == notification.Id);
        deleted.IsDeleted.Should().BeTrue();
        deleted.DeletedAt.Should().NotBeNull();
        (await context.Notifications.AnyAsync(n => n.Id == notification.Id)).Should().BeFalse();
    }

    [Test]
    public async Task DeleteAsync_OtherUserOrMissingNotification_ShouldThrowNotFoundException()
    {
        await using var context = NotificationTestFactory.CreateContext();
        var userA = NotificationTestFactory.User();
        var userB = NotificationTestFactory.User();
        var other = NotificationTestFactory.Notification(userB.Id, "Other");
        context.Users.AddRange(userA, userB);
        context.Notifications.Add(other);
        await context.SaveChangesAsync();
        var service = NotificationTestFactory.CreateService(context);

        await service.Invoking(s => s.DeleteAsync(userA.Id, other.Id))
            .Should().ThrowAsync<NotFoundException>();
        await service.Invoking(s => s.DeleteAsync(userA.Id, Guid.NewGuid()))
            .Should().ThrowAsync<NotFoundException>();
    }
}
