using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RealSync.Shared.DTOs.Requests.Leads;
using RealSync.Shared.Exceptions;
using ValidationException = RealSync.Shared.Exceptions.ValidationException;

namespace RealSync.UnitTests.Leads;

[TestFixture]
public class LeadServiceTests
{
    [Test]
    public async Task CreateLeadAsync_ValidRequest_ShouldCreateLeadWithDefaults()
    {
        await using var context = LeadTestFactory.CreateContext();
        var service = LeadTestFactory.CreateService(context);

        var result = await service.CreateLeadAsync(new LeadCreateDto
        {
            FullName = " Nguyen Van A ",
            Phone = " 0901234567 "
        });

        result.Status.Should().Be("New");
        result.Priority.Should().Be("Normal");
        result.Score.Should().Be(0);
        result.LeadTemperature.Should().Be("Cold");
        result.FullName.Should().Be("Nguyen Van A");
    }

    [Test]
    public async Task CreateLeadAsync_WithEmailOnly_ShouldCreateLead()
    {
        await using var context = LeadTestFactory.CreateContext();
        var service = LeadTestFactory.CreateService(context);

        var result = await service.CreateLeadAsync(new LeadCreateDto
        {
            FullName = "Email Lead",
            Email = "lead@example.com"
        });

        result.Email.Should().Be("lead@example.com");
        result.Phone.Should().BeNull();
    }

    [Test]
    public async Task CreateLeadAsync_InvalidInputs_ShouldThrowValidationException()
    {
        await using var context = LeadTestFactory.CreateContext();
        var service = LeadTestFactory.CreateService(context);

        await service.Invoking(s => s.CreateLeadAsync(new LeadCreateDto { FullName = "No Contact" }))
            .Should().ThrowAsync<ValidationException>();

        await service.Invoking(s => s.CreateLeadAsync(new LeadCreateDto { FullName = "Bad Score", Phone = "1", Score = 101 }))
            .Should().ThrowAsync<ValidationException>();

        await service.Invoking(s => s.CreateLeadAsync(new LeadCreateDto { FullName = "Bad Status", Phone = "1", Status = "InvalidStatus" }))
            .Should().ThrowAsync<ValidationException>();

        await service.Invoking(s => s.CreateLeadAsync(new LeadCreateDto { FullName = "Bad Priority", Phone = "1", Priority = "SuperHigh" }))
            .Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task CreateLeadAsync_WithInvalidReferences_ShouldThrowValidationException()
    {
        await using var context = LeadTestFactory.CreateContext();
        var inactiveUser = LeadTestFactory.InactiveUser();
        context.Users.Add(inactiveUser);
        await context.SaveChangesAsync();
        var service = LeadTestFactory.CreateService(context);

        await service.Invoking(s => s.CreateLeadAsync(new LeadCreateDto
        {
            FullName = "Invalid Assigned",
            Phone = "1",
            AssignedToId = inactiveUser.Id
        })).Should().ThrowAsync<ValidationException>();

        await service.Invoking(s => s.CreateLeadAsync(new LeadCreateDto
        {
            FullName = "Invalid Property",
            Phone = "1",
            InterestedPropertyId = Guid.NewGuid()
        })).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task GetLeadByIdAsync_ExistingLead_ShouldReturnDetail()
    {
        await using var context = LeadTestFactory.CreateContext();
        var lead = LeadTestFactory.Lead("Detail Lead", score: 75);
        context.Leads.Add(lead);
        await context.SaveChangesAsync();

        var result = await LeadTestFactory.CreateService(context).GetLeadByIdAsync(lead.Id);

        result.Id.Should().Be(lead.Id);
        result.FullName.Should().Be("Detail Lead");
        result.Status.Should().Be("New");
        result.LeadTemperature.Should().Be("Hot");
    }

    [Test]
    public async Task GetLeadByIdAsync_NotFound_ShouldThrowNotFoundException()
    {
        await using var context = LeadTestFactory.CreateContext();
        var service = LeadTestFactory.CreateService(context);

        await service.Invoking(s => s.GetLeadByIdAsync(Guid.NewGuid()))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task GetLeadsAsync_ShouldFilterSearchAndSort()
    {
        await using var context = LeadTestFactory.CreateContext();
        context.Leads.AddRange(
            LeadTestFactory.Lead("Alpha", "New", "High", 80),
            LeadTestFactory.Lead("Beta", "Contacted", "Normal", 30),
            LeadTestFactory.Lead("Gamma", "New", "High", 60));
        await context.SaveChangesAsync();

        var service = LeadTestFactory.CreateService(context);
        var paged = await service.GetLeadsAsync(new LeadQueryDto { Page = 1, PageSize = 2 });
        var status = await service.GetLeadsAsync(new LeadQueryDto { Status = "New" });
        var priority = await service.GetLeadsAsync(new LeadQueryDto { Priority = "High" });
        var search = await service.GetLeadsAsync(new LeadQueryDto { Search = "Alpha" });
        var sorted = await service.GetLeadsAsync(new LeadQueryDto { SortBy = "score", SortDirection = "desc" });

        paged.Items.Should().HaveCount(2);
        paged.TotalCount.Should().Be(3);
        status.Items.Should().OnlyContain(x => x.Status == "New");
        priority.Items.Should().OnlyContain(x => x.Priority == "High");
        search.Items.Should().ContainSingle(x => x.FullName == "Alpha");
        sorted.Items.Select(x => x.Score).Should().BeInDescendingOrder();
    }

    [Test]
    public async Task UpdateLeadAsync_ShouldUpdateAndValidate()
    {
        await using var context = LeadTestFactory.CreateContext();
        var lead = LeadTestFactory.Lead("Old Lead");
        var inactiveUser = LeadTestFactory.InactiveUser();
        context.Leads.Add(lead);
        context.Users.Add(inactiveUser);
        await context.SaveChangesAsync();
        var service = LeadTestFactory.CreateService(context);

        var updated = await service.UpdateLeadAsync(lead.Id, new LeadUpdateDto
        {
            FullName = "New Lead",
            Status = "Qualified",
            Priority = "Urgent",
            Score = 45
        });

        updated.FullName.Should().Be("New Lead");
        updated.Status.Should().Be("Qualified");
        updated.Priority.Should().Be("Urgent");
        updated.LeadTemperature.Should().Be("Warm");

        await service.Invoking(s => s.UpdateLeadAsync(Guid.NewGuid(), new LeadUpdateDto { FullName = "Missing" }))
            .Should().ThrowAsync<NotFoundException>();
        await service.Invoking(s => s.UpdateLeadAsync(lead.Id, new LeadUpdateDto { Score = -1 }))
            .Should().ThrowAsync<ValidationException>();
        await service.Invoking(s => s.UpdateLeadAsync(lead.Id, new LeadUpdateDto { Status = "Bad" }))
            .Should().ThrowAsync<ValidationException>();
        await service.Invoking(s => s.UpdateLeadAsync(lead.Id, new LeadUpdateDto { Priority = "Bad" }))
            .Should().ThrowAsync<ValidationException>();
        await service.Invoking(s => s.UpdateLeadAsync(lead.Id, new LeadUpdateDto { AssignedToId = inactiveUser.Id }))
            .Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task DeleteLeadAsync_ExistingLead_ShouldSoftDeleteLead()
    {
        await using var context = LeadTestFactory.CreateContext();
        var lead = LeadTestFactory.Lead("Delete Lead");
        context.Leads.Add(lead);
        await context.SaveChangesAsync();

        var service = LeadTestFactory.CreateService(context);
        await service.DeleteLeadAsync(lead.Id);

        var deleted = await context.Leads.IgnoreQueryFilters().FirstAsync(x => x.Id == lead.Id);
        deleted.IsDeleted.Should().BeTrue();
        deleted.DeletedAt.Should().NotBeNull();
        (await context.Leads.AnyAsync(x => x.Id == lead.Id)).Should().BeFalse();

        await service.Invoking(s => s.DeleteLeadAsync(Guid.NewGuid()))
            .Should().ThrowAsync<NotFoundException>();
    }
}
