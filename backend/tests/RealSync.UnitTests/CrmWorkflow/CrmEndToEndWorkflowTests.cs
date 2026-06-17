using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RealSync.Core.Entities;
using RealSync.Core.Interfaces;
using RealSync.Data.Context;
using RealSync.Data.Repositories;
using RealSync.Services.Implementations;
using RealSync.Shared.DTOs.Requests.CrmAnalytics;
using RealSync.Shared.DTOs.Requests.Leads;

namespace RealSync.UnitTests.CrmWorkflow;

[TestFixture]
public class CrmEndToEndWorkflowTests
{
    [Test]
    public async Task LeadToCustomerWorkflow_ShouldCreateNotificationsActivitiesCustomerAndAnalytics()
    {
        await using var context = CreateContext();
        var assignedUser = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Workflow Agent",
            Email = "workflow.agent@realsync.vn",
            PasswordHash = "hash",
            RoleId = Guid.NewGuid(),
            IsActive = true
        };
        context.Users.Add(assignedUser);
        await context.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(x => x.UserId).Returns(assignedUser.Id);
        var notificationService = new NotificationService(context);
        var leadService = new LeadService(
            new LeadRepository(context),
            new CustomerRepository(context),
            context,
            Mock.Of<IActivityLogService>(),
            notificationService,
            currentUser.Object,
            NullLogger<LeadService>.Instance);
        var analyticsService = new CrmAnalyticsService(context, NullLogger<CrmAnalyticsService>.Instance);

        var created = await leadService.CreateLeadAsync(new LeadCreateDto
        {
            FullName = "Workflow Lead",
            Phone = "0901234567",
            Score = 80,
            SourceChannel = "Website"
        });
        await leadService.UpdateStatusAsync(created.Id, new LeadStatusUpdateDto { Status = "Qualified" });
        await leadService.AssignLeadAsync(created.Id, new LeadAssignDto { AssignedToId = assignedUser.Id });
        await leadService.AddActivityAsync(created.Id, new LeadActivityCreateDto { ActivityType = "Call", Description = "Intro call" });
        await leadService.SetFollowUpAsync(created.Id, new LeadFollowUpDto { NextFollowUpAt = DateTime.UtcNow.AddDays(2) });
        var customer = await leadService.ConvertToCustomerAsync(created.Id, new LeadConvertToCustomerDto { Company = "Workflow Co" });

        var lead = await context.Leads.FirstAsync(l => l.Id == created.Id);
        lead.Status.Should().Be("Won");
        lead.ConvertedAt.Should().NotBeNull();
        customer.ConvertedFromLeadId.Should().Be(created.Id);
        (await context.Customers.CountAsync(c => c.ConvertedFromLeadId == created.Id)).Should().Be(1);
        (await context.LeadActivities.CountAsync(a => a.LeadId == created.Id)).Should().BeGreaterThanOrEqualTo(4);
        (await context.Notifications.CountAsync(n => n.UserId == assignedUser.Id)).Should().BeGreaterThanOrEqualTo(2);

        var summary = await analyticsService.GetSummaryAsync(new CrmAnalyticsQueryDto());
        summary.TotalLeads.Should().Be(1);
        summary.WonLeads.Should().Be(1);
        summary.HotLeads.Should().Be(1);
        summary.TotalCustomers.Should().Be(1);
        summary.CustomersFromLeads.Should().Be(1);
        summary.LeadToWonConversionRate.Should().Be(100);
        summary.LeadToCustomerConversionRate.Should().Be(100);
    }

    private static RealSyncDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RealSyncDbContext>()
            .UseInMemoryDatabase($"realsync-crm-e2e-{Guid.NewGuid():N}")
            .Options;

        return new RealSyncDbContext(options);
    }
}
