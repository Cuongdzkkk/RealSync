using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RealSync.Data.Repositories;
using RealSync.Shared.DTOs.Requests.Leads;

namespace RealSync.UnitTests.Leads;

[TestFixture]
public class LeadRepositoryTests
{
    [Test]
    public async Task GetPagedAsync_ShouldApplyPaginationSearchStatusAndScoreFilters()
    {
        await using var context = LeadTestFactory.CreateContext();
        context.Leads.AddRange(
            LeadTestFactory.Lead("Searchable Lead", "New", "Normal", 90),
            LeadTestFactory.Lead("Other Lead", "Contacted", "Normal", 30),
            LeadTestFactory.Lead("Low Lead", "New", "High", 20));
        await context.SaveChangesAsync();
        var repository = new LeadRepository(context);

        var paged = await repository.GetPagedAsync(new LeadQueryDto { Page = 1, PageSize = 2 });
        var search = await repository.GetPagedAsync(new LeadQueryDto { Search = "Searchable" });
        var status = await repository.GetPagedAsync(new LeadQueryDto { Status = "New" });
        var score = await repository.GetPagedAsync(new LeadQueryDto { MinScore = 80, MaxScore = 100 });

        paged.Items.Should().HaveCount(2);
        paged.TotalCount.Should().Be(3);
        search.Items.Should().ContainSingle(x => x.FullName == "Searchable Lead");
        status.Items.Should().OnlyContain(x => x.Status == "New");
        score.Items.Should().ContainSingle(x => x.Score == 90);
    }

    [Test]
    public async Task GetDetailByIdAsync_ShouldIncludeAssignedToAndInterestedProperty()
    {
        await using var context = LeadTestFactory.CreateContext();
        var user = LeadTestFactory.ActiveUser();
        var property = LeadTestFactory.Property();
        var lead = LeadTestFactory.Lead("Included Lead");
        lead.AssignedToId = user.Id;
        lead.InterestedPropertyId = property.Id;

        context.Users.Add(user);
        context.Properties.Add(property);
        context.Leads.Add(lead);
        await context.SaveChangesAsync();

        var result = await new LeadRepository(context).GetDetailByIdAsync(lead.Id);

        result.Should().NotBeNull();
        result!.AssignedTo.Should().NotBeNull();
        result.InterestedProperty.Should().NotBeNull();
        result.AssignedTo!.FullName.Should().Be(user.FullName);
        result.InterestedProperty!.Title.Should().Be(property.Title);
    }

    [Test]
    public async Task DeleteAsync_ShouldSetIsDeletedAndDeletedAt()
    {
        await using var context = LeadTestFactory.CreateContext();
        var lead = LeadTestFactory.Lead("Repo Delete");
        context.Leads.Add(lead);
        await context.SaveChangesAsync();

        await new LeadRepository(context).DeleteAsync(lead);

        var deleted = await context.Leads.IgnoreQueryFilters().FirstAsync(x => x.Id == lead.Id);
        deleted.IsDeleted.Should().BeTrue();
        deleted.DeletedAt.Should().NotBeNull();
    }

    [Test]
    public async Task AddActivityAsync_ShouldPersistActivity()
    {
        await using var context = LeadTestFactory.CreateContext();
        var lead = LeadTestFactory.Lead("Activity Lead");
        context.Leads.Add(lead);
        await context.SaveChangesAsync();

        var activity = await new LeadRepository(context).AddActivityAsync(new()
        {
            LeadId = lead.Id,
            ActivityType = "Call",
            Description = "Called"
        });

        activity.Id.Should().NotBeEmpty();
        (await context.LeadActivities.CountAsync(a => a.LeadId == lead.Id)).Should().Be(1);
    }

    [Test]
    public async Task GetActivitiesAsync_ShouldReturnActivitiesByLeadIdOnlyAndOrderDesc()
    {
        await using var context = LeadTestFactory.CreateContext();
        var lead = LeadTestFactory.Lead("Activity Lead");
        var otherLead = LeadTestFactory.Lead("Other Lead");
        context.Leads.AddRange(lead, otherLead);
        context.LeadActivities.AddRange(
            new() { LeadId = lead.Id, ActivityType = "Note", CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new() { LeadId = lead.Id, ActivityType = "Call", CreatedAt = DateTime.UtcNow },
            new() { LeadId = otherLead.Id, ActivityType = "Email", CreatedAt = DateTime.UtcNow.AddDays(1) });
        await context.SaveChangesAsync();

        var activities = await new LeadRepository(context).GetActivitiesAsync(lead.Id);

        activities.Should().HaveCount(2);
        activities.Should().OnlyContain(a => a.LeadId == lead.Id);
        activities.Select(a => a.CreatedAt).Should().BeInDescendingOrder();
    }

    [Test]
    public async Task GetPagedAsync_OverdueFollowUp_ShouldReturnOnlyOverdueLeads()
    {
        await using var context = LeadTestFactory.CreateContext();
        var overdue = LeadTestFactory.Lead("Overdue");
        overdue.NextFollowUpAt = DateTime.UtcNow.AddDays(-1);
        var future = LeadTestFactory.Lead("Future");
        future.NextFollowUpAt = DateTime.UtcNow.AddDays(1);
        var won = LeadTestFactory.Lead("Won", "Won");
        won.NextFollowUpAt = DateTime.UtcNow.AddDays(-1);
        context.Leads.AddRange(overdue, future, won);
        await context.SaveChangesAsync();

        var result = await new LeadRepository(context).GetPagedAsync(new LeadQueryDto { OverdueFollowUp = true });

        result.Items.Should().ContainSingle(x => x.Id == overdue.Id);
    }

    [Test]
    public async Task GetPagedAsync_FollowUpRange_ShouldFilterByNextFollowUpAt()
    {
        await using var context = LeadTestFactory.CreateContext();
        var inRange = LeadTestFactory.Lead("In Range");
        inRange.NextFollowUpAt = DateTime.UtcNow.AddDays(2);
        var outRange = LeadTestFactory.Lead("Out Range");
        outRange.NextFollowUpAt = DateTime.UtcNow.AddDays(5);
        context.Leads.AddRange(inRange, outRange);
        await context.SaveChangesAsync();

        var result = await new LeadRepository(context).GetPagedAsync(new LeadQueryDto
        {
            FollowUpFrom = DateTime.UtcNow.AddDays(1),
            FollowUpTo = DateTime.UtcNow.AddDays(3)
        });

        result.Items.Should().ContainSingle(x => x.Id == inRange.Id);
    }
}
