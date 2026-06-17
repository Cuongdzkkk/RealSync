using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RealSync.Core.Entities;
using RealSync.Core.Enums;

namespace RealSync.UnitTests.FollowUpReminders;

[TestFixture]
public class FollowUpReminderServiceTests
{
    [Test]
    public async Task DueAssignedLead_ShouldCreateNotificationAndDispatch()
    {
        await using var store = FollowUpReminderTestFactory.CreateSqliteContext();
        var (user, lead) = await SeedDueLeadAsync(store.Context);

        var result = await FollowUpReminderTestFactory.CreateService(store.Context).ProcessDueRemindersAsync();

        result.Scanned.Should().Be(1);
        result.Sent.Should().Be(1);
        var notification = await store.Context.Notifications.SingleAsync();
        notification.UserId.Should().Be(user.Id);
        notification.Title.Should().Be("Đến giờ chăm sóc lead");
        notification.Message.Should().Be($"Đã đến lịch chăm sóc lead {lead.FullName}.");
        notification.Type.Should().Be(NotificationType.Lead);
        notification.Link.Should().Be($"/leads/{lead.Id}");
        var dispatch = await store.Context.FollowUpReminderDispatches.SingleAsync();
        dispatch.LeadId.Should().Be(lead.Id);
        dispatch.NotificationId.Should().Be(notification.Id);
        dispatch.ScheduledFor.Should().Be(lead.NextFollowUpAt!.Value);
    }

    [Test]
    public async Task FutureFollowUp_ShouldNotSend()
    {
        await using var store = FollowUpReminderTestFactory.CreateSqliteContext();
        await SeedLeadAsync(store.Context, nextFollowUpAt: DateTime.UtcNow.AddHours(1));

        var result = await FollowUpReminderTestFactory.CreateService(store.Context).ProcessDueRemindersAsync();

        result.Scanned.Should().Be(0);
        (await store.Context.Notifications.CountAsync()).Should().Be(0);
        (await store.Context.FollowUpReminderDispatches.CountAsync()).Should().Be(0);
    }

    [TestCase("Won")]
    [TestCase("Lost")]
    public async Task ClosedLead_ShouldNotSend(string status)
    {
        await using var store = FollowUpReminderTestFactory.CreateSqliteContext();
        await SeedLeadAsync(store.Context, DateTime.UtcNow.AddMinutes(-1), status);

        var result = await FollowUpReminderTestFactory.CreateService(store.Context).ProcessDueRemindersAsync();

        result.Scanned.Should().Be(0);
    }

    [Test]
    public async Task UnassignedLead_ShouldNotSend()
    {
        await using var store = FollowUpReminderTestFactory.CreateSqliteContext();
        store.Context.Leads.Add(FollowUpReminderTestFactory.Lead(null, DateTime.UtcNow.AddMinutes(-1)));
        await store.Context.SaveChangesAsync();

        var result = await FollowUpReminderTestFactory.CreateService(store.Context).ProcessDueRemindersAsync();

        result.Scanned.Should().Be(0);
    }

    [Test]
    public async Task ProcessTwice_ShouldNotCreateDuplicate()
    {
        await using var store = FollowUpReminderTestFactory.CreateSqliteContext();
        await SeedDueLeadAsync(store.Context);
        var service = FollowUpReminderTestFactory.CreateService(store.Context);

        var first = await service.ProcessDueRemindersAsync();
        var second = await service.ProcessDueRemindersAsync();

        first.Sent.Should().Be(1);
        second.Sent.Should().Be(0);
        second.Skipped.Should().Be(1);
        (await store.Context.Notifications.CountAsync()).Should().Be(1);
        (await store.Context.FollowUpReminderDispatches.CountAsync()).Should().Be(1);
    }

    [Test]
    public async Task ExistingDispatch_ShouldSkip()
    {
        await using var store = FollowUpReminderTestFactory.CreateSqliteContext();
        var (_, lead) = await SeedDueLeadAsync(store.Context);
        var notification = new Notification
        {
            UserId = lead.AssignedToId!.Value,
            Title = "Existing",
            Message = "Existing",
            Type = NotificationType.Lead
        };
        store.Context.Notifications.Add(notification);
        store.Context.FollowUpReminderDispatches.Add(new FollowUpReminderDispatch
        {
            LeadId = lead.Id,
            ScheduledFor = lead.NextFollowUpAt!.Value,
            NotificationId = notification.Id,
            SentAt = DateTime.UtcNow
        });
        await store.Context.SaveChangesAsync();

        var result = await FollowUpReminderTestFactory.CreateService(store.Context).ProcessDueRemindersAsync();

        result.Sent.Should().Be(0);
        result.Skipped.Should().Be(1);
        (await store.Context.Notifications.CountAsync()).Should().Be(1);
    }

    [Test]
    public async Task RescheduledFollowUp_ShouldCreateNewReminder()
    {
        await using var store = FollowUpReminderTestFactory.CreateSqliteContext();
        var (_, lead) = await SeedDueLeadAsync(store.Context);
        var oldSchedule = DateTime.UtcNow.AddDays(-1);
        var oldNotification = new Notification
        {
            UserId = lead.AssignedToId!.Value,
            Title = "Old",
            Message = "Old",
            Type = NotificationType.Lead
        };
        store.Context.Notifications.Add(oldNotification);
        store.Context.FollowUpReminderDispatches.Add(new FollowUpReminderDispatch
        {
            LeadId = lead.Id,
            ScheduledFor = oldSchedule,
            NotificationId = oldNotification.Id,
            SentAt = DateTime.UtcNow.AddDays(-1)
        });
        await store.Context.SaveChangesAsync();

        var result = await FollowUpReminderTestFactory.CreateService(store.Context).ProcessDueRemindersAsync();

        result.Sent.Should().Be(1);
        (await store.Context.Notifications.CountAsync()).Should().Be(2);
        (await store.Context.FollowUpReminderDispatches.CountAsync()).Should().Be(2);
    }

    [Test]
    public async Task Notification_ShouldContainCorrectStructuredData()
    {
        await using var store = FollowUpReminderTestFactory.CreateSqliteContext();
        var (user, lead) = await SeedDueLeadAsync(store.Context);

        await FollowUpReminderTestFactory.CreateService(store.Context).ProcessDueRemindersAsync();

        var notification = await store.Context.Notifications.SingleAsync();
        using var json = JsonDocument.Parse(notification.Data!);
        json.RootElement.GetProperty("eventType").GetString().Should().Be("FollowUpDue");
        json.RootElement.GetProperty("leadId").GetGuid().Should().Be(lead.Id);
        json.RootElement.GetProperty("assignedToId").GetGuid().Should().Be(user.Id);
        json.RootElement.GetProperty("scheduledFor").GetDateTime().Should().Be(lead.NextFollowUpAt!.Value);
    }

    [Test]
    public async Task ShouldIgnoreSoftDeletedLead()
    {
        await using var store = FollowUpReminderTestFactory.CreateSqliteContext();
        await SeedLeadAsync(store.Context, DateTime.UtcNow.AddMinutes(-1), isDeleted: true);

        var result = await FollowUpReminderTestFactory.CreateService(store.Context).ProcessDueRemindersAsync();

        result.Scanned.Should().Be(0);
    }

    [Test]
    public async Task ShouldRespectBatchSize()
    {
        await using var store = FollowUpReminderTestFactory.CreateSqliteContext();
        await SeedLeadAsync(store.Context, DateTime.UtcNow.AddMinutes(-3));
        await SeedLeadAsync(store.Context, DateTime.UtcNow.AddMinutes(-2));
        await SeedLeadAsync(store.Context, DateTime.UtcNow.AddMinutes(-1));

        var result = await FollowUpReminderTestFactory.CreateService(store.Context, batchSize: 2).ProcessDueRemindersAsync();

        result.Scanned.Should().Be(2);
        result.Sent.Should().Be(2);
        (await store.Context.Notifications.CountAsync()).Should().Be(2);
    }

    [Test]
    public async Task UniqueConstraint_ShouldPreventDuplicateDispatch()
    {
        await using var store = FollowUpReminderTestFactory.CreateSqliteContext();
        var (_, lead) = await SeedDueLeadAsync(store.Context);
        var notification1 = new Notification { UserId = lead.AssignedToId!.Value, Title = "N1", Message = "N1", Type = NotificationType.Lead };
        var notification2 = new Notification { UserId = lead.AssignedToId!.Value, Title = "N2", Message = "N2", Type = NotificationType.Lead };
        store.Context.Notifications.AddRange(notification1, notification2);
        store.Context.FollowUpReminderDispatches.AddRange(
            new FollowUpReminderDispatch { LeadId = lead.Id, ScheduledFor = lead.NextFollowUpAt!.Value, NotificationId = notification1.Id, SentAt = DateTime.UtcNow },
            new FollowUpReminderDispatch { LeadId = lead.Id, ScheduledFor = lead.NextFollowUpAt!.Value, NotificationId = notification2.Id, SentAt = DateTime.UtcNow });

        await store.Context.Invoking(c => c.SaveChangesAsync())
            .Should().ThrowAsync<DbUpdateException>();
    }

    private static async Task<(User User, Lead Lead)> SeedDueLeadAsync(RealSync.Data.Context.RealSyncDbContext context)
    {
        return await SeedLeadAsync(context, DateTime.UtcNow.AddMinutes(-1));
    }

    private static async Task<(User User, Lead Lead)> SeedLeadAsync(
        RealSync.Data.Context.RealSyncDbContext context,
        DateTime? nextFollowUpAt,
        string status = "Contacted",
        bool isDeleted = false)
    {
        var role = FollowUpReminderTestFactory.Role();
        var user = FollowUpReminderTestFactory.User(role.Id);
        var lead = FollowUpReminderTestFactory.Lead(user.Id, nextFollowUpAt, status, isDeleted);
        context.Roles.Add(role);
        context.Users.Add(user);
        context.Leads.Add(lead);
        await context.SaveChangesAsync();
        return (user, lead);
    }
}
