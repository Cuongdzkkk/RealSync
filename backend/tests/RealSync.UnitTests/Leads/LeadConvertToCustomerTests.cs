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
public class LeadConvertToCustomerTests
{
    [Test]
    public async Task ConvertToCustomerAsync_ValidLead_ShouldCreateCustomerAndMarkLeadWon()
    {
        await using var context = LeadTestFactory.CreateContext();
        var activityLog = new Mock<IActivityLogService>();
        var lead = LeadTestFactory.Lead("Convert Lead", "Qualified");
        lead.Requirements = "Need apartment";
        lead.SourceChannel = "Website";
        context.Leads.Add(lead);
        await context.SaveChangesAsync();
        var service = LeadTestFactory.CreateService(context, activityLogService: activityLog);

        var result = await service.ConvertToCustomerAsync(lead.Id, new LeadConvertToCustomerDto
        {
            Company = "RealSync"
        });

        result.FullName.Should().Be(lead.FullName);
        result.Company.Should().Be("RealSync");
        result.Source.Should().Be("Website");
        result.ConvertedFromLeadId.Should().Be(lead.Id);

        var updatedLead = await context.Leads.FirstAsync(l => l.Id == lead.Id);
        updatedLead.Status.Should().Be("Won");
        updatedLead.ConvertedAt.Should().NotBeNull();

        var customer = await context.Customers.SingleAsync(c => c.ConvertedFromLeadId == lead.Id);
        customer.Notes.Should().Be("Need apartment");
        var activities = await context.LeadActivities.Where(a => a.LeadId == lead.Id).ToListAsync();
        activities.Should().Contain(a => a.ActivityType == "StatusChange");
        activities.Should().Contain(a => a.ActivityType == "Converted" && a.NewValue == customer.Id.ToString());
        activityLog.Verify(l => l.LogAsync("Lead", lead.Id, ActivityType.Update, It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<object?>()), Times.Once);
        activityLog.Verify(l => l.LogAsync("Customer", customer.Id, ActivityType.Create, It.IsAny<string>(), null, It.IsAny<object?>()), Times.Once);
    }

    [Test]
    public async Task ConvertToCustomerAsync_ShouldSupportOverrides()
    {
        await using var context = LeadTestFactory.CreateContext();
        var user = LeadTestFactory.ActiveUser();
        var lead = LeadTestFactory.Lead("Convert Lead", "Won");
        lead.ConvertedAt = DateTime.UtcNow.AddDays(-1);
        context.Users.Add(user);
        context.Leads.Add(lead);
        await context.SaveChangesAsync();

        var result = await LeadTestFactory.CreateService(context).ConvertToCustomerAsync(lead.Id, new LeadConvertToCustomerDto
        {
            FullName = " Customer Override ",
            Phone = " 0912345678 ",
            Email = " override@example.com ",
            Address = " District 1 ",
            Notes = "VIP",
            Source = "Referral",
            AssignedToId = user.Id
        });

        result.FullName.Should().Be("Customer Override");
        result.Phone.Should().Be("0912345678");
        result.Email.Should().Be("override@example.com");
        result.Address.Should().Be("District 1");
        result.AssignedToId.Should().Be(user.Id);
        (await context.LeadActivities.CountAsync(a => a.LeadId == lead.Id && a.ActivityType == "StatusChange")).Should().Be(0);
        (await context.LeadActivities.CountAsync(a => a.LeadId == lead.Id && a.ActivityType == "Converted")).Should().Be(1);
    }

    [Test]
    public async Task ConvertToCustomerAsync_DuplicateOrMissingLead_ShouldThrow()
    {
        await using var context = LeadTestFactory.CreateContext();
        var lead = LeadTestFactory.Lead("Convert Lead");
        context.Leads.Add(lead);
        await context.SaveChangesAsync();
        var service = LeadTestFactory.CreateService(context);

        await service.ConvertToCustomerAsync(lead.Id, new LeadConvertToCustomerDto());

        await service.Invoking(s => s.ConvertToCustomerAsync(lead.Id, new LeadConvertToCustomerDto()))
            .Should().ThrowAsync<ValidationException>();
        await service.Invoking(s => s.ConvertToCustomerAsync(Guid.NewGuid(), new LeadConvertToCustomerDto()))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ConvertToCustomerAsync_InvalidAssignedUser_ShouldThrow()
    {
        await using var context = LeadTestFactory.CreateContext();
        var lead = LeadTestFactory.Lead("Convert Lead");
        var inactiveUser = LeadTestFactory.InactiveUser();
        context.Leads.Add(lead);
        context.Users.Add(inactiveUser);
        await context.SaveChangesAsync();

        await LeadTestFactory.CreateService(context)
            .Invoking(s => s.ConvertToCustomerAsync(lead.Id, new LeadConvertToCustomerDto { AssignedToId = inactiveUser.Id }))
            .Should().ThrowAsync<ValidationException>();
    }
}
