using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using RealSync.Core.Enums;
using RealSync.Core.Interfaces;
using RealSync.Shared.DTOs.Requests.Leads;
using RealSync.Shared.Exceptions;
using ValidationException = RealSync.Shared.Exceptions.ValidationException;

namespace RealSync.UnitTests.Leads;

[TestFixture]
public class LeadWorkflowServiceTests
{
    [Test]
    public async Task UpdateStatusAsync_ValidStatus_ShouldUpdateLeadStatusAndAddActivity()
    {
        await using var context = LeadTestFactory.CreateContext();
        var lead = LeadTestFactory.Lead("Status Lead", "New");
        context.Leads.Add(lead);
        await context.SaveChangesAsync();

        var result = await LeadTestFactory.CreateService(context)
            .UpdateStatusAsync(lead.Id, new LeadStatusUpdateDto { Status = "Contacted", Note = "Called first time" });

        result.Status.Should().Be("Contacted");
        var activity = await context.LeadActivities.SingleAsync(a => a.LeadId == lead.Id);
        activity.ActivityType.Should().Be("StatusChange");
        activity.OldValue.Should().Be("New");
        activity.NewValue.Should().Be("Contacted");
    }

    [Test]
    public async Task UpdateStatusAsync_InvalidOrMissingLead_ShouldThrow()
    {
        await using var context = LeadTestFactory.CreateContext();
        var lead = LeadTestFactory.Lead("Status Lead");
        context.Leads.Add(lead);
        await context.SaveChangesAsync();
        var service = LeadTestFactory.CreateService(context);

        await service.Invoking(s => s.UpdateStatusAsync(lead.Id, new LeadStatusUpdateDto { Status = "InvalidStatus" }))
            .Should().ThrowAsync<ValidationException>();

        await service.Invoking(s => s.UpdateStatusAsync(Guid.NewGuid(), new LeadStatusUpdateDto { Status = "Contacted" }))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task UpdateStatusAsync_ToWon_ShouldSetConvertedAt()
    {
        await using var context = LeadTestFactory.CreateContext();
        var lead = LeadTestFactory.Lead("Won Lead", "Proposal");
        context.Leads.Add(lead);
        await context.SaveChangesAsync();

        await LeadTestFactory.CreateService(context)
            .UpdateStatusAsync(lead.Id, new LeadStatusUpdateDto { Status = "Won" });

        var updated = await context.Leads.FirstAsync(l => l.Id == lead.Id);
        updated.Status.Should().Be("Won");
        updated.ConvertedAt.Should().NotBeNull();
        context.Customers.Should().BeEmpty();
    }

    [Test]
    public async Task AssignLeadAsync_ValidUser_ShouldAssignLeadAndSendNotification()
    {
        await using var context = LeadTestFactory.CreateContext();
        var lead = LeadTestFactory.Lead("Assign Lead");
        var user = LeadTestFactory.ActiveUser();
        context.Leads.Add(lead);
        context.Users.Add(user);
        await context.SaveChangesAsync();
        var notifications = new Mock<INotificationService>();

        var result = await LeadTestFactory.CreateService(context, notifications)
            .AssignLeadAsync(lead.Id, new LeadAssignDto { AssignedToId = user.Id, Note = "Assign to agent" });

        result.AssignedToId.Should().Be(user.Id);
        (await context.LeadActivities.SingleAsync(a => a.LeadId == lead.Id)).ActivityType.Should().Be("Assigned");
        notifications.Verify(n => n.SendAsync(
            user.Id,
            It.IsAny<string>(),
            It.IsAny<string>(),
            NotificationType.Assignment,
            It.IsAny<string?>(),
            It.IsAny<object?>()), Times.Once);
    }

    [Test]
    public async Task AssignLeadAsync_InvalidInactiveOrMissingLead_ShouldThrow()
    {
        await using var context = LeadTestFactory.CreateContext();
        var lead = LeadTestFactory.Lead("Assign Lead");
        var inactiveUser = LeadTestFactory.InactiveUser();
        context.Leads.Add(lead);
        context.Users.Add(inactiveUser);
        await context.SaveChangesAsync();
        var service = LeadTestFactory.CreateService(context);

        await service.Invoking(s => s.AssignLeadAsync(lead.Id, new LeadAssignDto { AssignedToId = Guid.NewGuid() }))
            .Should().ThrowAsync<ValidationException>();
        await service.Invoking(s => s.AssignLeadAsync(lead.Id, new LeadAssignDto { AssignedToId = inactiveUser.Id }))
            .Should().ThrowAsync<ValidationException>();
        await service.Invoking(s => s.AssignLeadAsync(Guid.NewGuid(), new LeadAssignDto { AssignedToId = inactiveUser.Id }))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task AssignLeadAsync_NotificationFails_ShouldStillAssignLead()
    {
        await using var context = LeadTestFactory.CreateContext();
        var lead = LeadTestFactory.Lead("Assign Lead");
        var user = LeadTestFactory.ActiveUser();
        context.Leads.Add(lead);
        context.Users.Add(user);
        await context.SaveChangesAsync();
        var notifications = new Mock<INotificationService>();
        notifications.Setup(n => n.SendAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<NotificationType>(),
                It.IsAny<string?>(),
                It.IsAny<object?>()))
            .ThrowsAsync(new InvalidOperationException("notification down"));

        var result = await LeadTestFactory.CreateService(context, notifications)
            .AssignLeadAsync(lead.Id, new LeadAssignDto { AssignedToId = user.Id });

        result.AssignedToId.Should().Be(user.Id);
        (await context.Leads.FirstAsync(l => l.Id == lead.Id)).AssignedToId.Should().Be(user.Id);
    }

    [Test]
    public async Task AddActivityAsync_Call_ShouldCreateActivityAndUpdateLastContactedAt()
    {
        await using var context = LeadTestFactory.CreateContext();
        var lead = LeadTestFactory.Lead("Activity Lead");
        context.Leads.Add(lead);
        await context.SaveChangesAsync();

        var activity = await LeadTestFactory.CreateService(context)
            .AddActivityAsync(lead.Id, new LeadActivityCreateDto { ActivityType = "Call", Description = "Called customer" });

        activity.ActivityType.Should().Be("Call");
        (await context.Leads.FirstAsync(l => l.Id == lead.Id)).LastContactedAt.Should().NotBeNull();
    }

    [Test]
    public async Task AddActivityAsync_Note_ShouldCreateActivity()
    {
        await using var context = LeadTestFactory.CreateContext();
        var lead = LeadTestFactory.Lead("Note Lead");
        context.Leads.Add(lead);
        await context.SaveChangesAsync();

        var activity = await LeadTestFactory.CreateService(context)
            .AddActivityAsync(lead.Id, new LeadActivityCreateDto { ActivityType = "Note", Description = "Internal note" });

        activity.ActivityType.Should().Be("Note");
        activity.Description.Should().Be("Internal note");
    }

    [Test]
    public async Task AddActivityAsync_InvalidInputs_ShouldThrow()
    {
        await using var context = LeadTestFactory.CreateContext();
        var lead = LeadTestFactory.Lead("Activity Lead");
        context.Leads.Add(lead);
        await context.SaveChangesAsync();
        var service = LeadTestFactory.CreateService(context);

        await service.Invoking(s => s.AddActivityAsync(lead.Id, new LeadActivityCreateDto { ActivityType = "Note" }))
            .Should().ThrowAsync<ValidationException>();
        await service.Invoking(s => s.AddActivityAsync(lead.Id, new LeadActivityCreateDto { ActivityType = "StatusChange", Description = "bad" }))
            .Should().ThrowAsync<ValidationException>();
        await service.Invoking(s => s.AddActivityAsync(Guid.NewGuid(), new LeadActivityCreateDto { ActivityType = "Call" }))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task GetActivitiesAsync_ExistingLead_ShouldReturnActivitiesOrderedDesc()
    {
        await using var context = LeadTestFactory.CreateContext();
        var lead = LeadTestFactory.Lead("Activities Lead");
        context.Leads.Add(lead);
        context.LeadActivities.AddRange(
            new() { LeadId = lead.Id, ActivityType = "Note", Description = "old", CreatedAt = DateTime.UtcNow.AddHours(-2) },
            new() { LeadId = lead.Id, ActivityType = "Call", Description = "new", CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var activities = await LeadTestFactory.CreateService(context).GetActivitiesAsync(lead.Id);

        activities.Should().HaveCount(2);
        activities.Select(a => a.CreatedAt).Should().BeInDescendingOrder();

        await LeadTestFactory.CreateService(context).Invoking(s => s.GetActivitiesAsync(Guid.NewGuid()))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task SetFollowUpAsync_ValidFutureDate_ShouldUpdateFollowUpAndCreateActivity()
    {
        await using var context = LeadTestFactory.CreateContext();
        var lead = LeadTestFactory.Lead("Follow Lead");
        context.Leads.Add(lead);
        await context.SaveChangesAsync();
        var next = DateTime.UtcNow.AddDays(1);

        var result = await LeadTestFactory.CreateService(context)
            .SetFollowUpAsync(lead.Id, new LeadFollowUpDto { NextFollowUpAt = next, Note = "Call back" });

        result.NextFollowUpAt.Should().Be(next);
        (await context.LeadActivities.SingleAsync(a => a.LeadId == lead.Id)).ActivityType.Should().Be("FollowUp");
    }

    [Test]
    public async Task SetFollowUpAsync_InvalidOrMissingLead_ShouldThrow()
    {
        await using var context = LeadTestFactory.CreateContext();
        var lead = LeadTestFactory.Lead("Follow Lead");
        context.Leads.Add(lead);
        await context.SaveChangesAsync();
        var service = LeadTestFactory.CreateService(context);

        await service.Invoking(s => s.SetFollowUpAsync(lead.Id, new LeadFollowUpDto { NextFollowUpAt = DateTime.UtcNow.AddMinutes(-1) }))
            .Should().ThrowAsync<ValidationException>();
        await service.Invoking(s => s.SetFollowUpAsync(Guid.NewGuid(), new LeadFollowUpDto { NextFollowUpAt = DateTime.UtcNow.AddDays(1) }))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task SetFollowUpAsync_WithAssignedUser_ShouldSendNotification()
    {
        await using var context = LeadTestFactory.CreateContext();
        var user = LeadTestFactory.ActiveUser();
        var lead = LeadTestFactory.Lead("Follow Lead");
        lead.AssignedToId = user.Id;
        context.Users.Add(user);
        context.Leads.Add(lead);
        await context.SaveChangesAsync();
        var notifications = new Mock<INotificationService>();

        await LeadTestFactory.CreateService(context, notifications)
            .SetFollowUpAsync(lead.Id, new LeadFollowUpDto { NextFollowUpAt = DateTime.UtcNow.AddDays(1) });

        notifications.Verify(n => n.SendAsync(
            user.Id,
            It.IsAny<string>(),
            It.IsAny<string>(),
            NotificationType.Lead,
            It.IsAny<string?>(),
            It.IsAny<object?>()), Times.Once);
    }

    [Test]
    public async Task SetFollowUpAsync_NotificationFails_ShouldStillUpdateFollowUp()
    {
        await using var context = LeadTestFactory.CreateContext();
        var user = LeadTestFactory.ActiveUser();
        var lead = LeadTestFactory.Lead("Follow Lead");
        lead.AssignedToId = user.Id;
        context.Users.Add(user);
        context.Leads.Add(lead);
        await context.SaveChangesAsync();
        var notifications = new Mock<INotificationService>();
        notifications.Setup(n => n.SendAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<NotificationType>(),
                It.IsAny<string?>(),
                It.IsAny<object?>()))
            .ThrowsAsync(new InvalidOperationException("notification down"));
        var next = DateTime.UtcNow.AddDays(1);

        var result = await LeadTestFactory.CreateService(context, notifications)
            .SetFollowUpAsync(lead.Id, new LeadFollowUpDto { NextFollowUpAt = next });

        result.NextFollowUpAt.Should().Be(next);
    }
}
