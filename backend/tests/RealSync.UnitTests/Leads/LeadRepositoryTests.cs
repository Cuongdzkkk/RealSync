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
}
