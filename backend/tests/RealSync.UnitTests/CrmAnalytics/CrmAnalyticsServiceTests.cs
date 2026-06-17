using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RealSync.Shared.DTOs.Requests.CrmAnalytics;

namespace RealSync.UnitTests.CrmAnalytics;

[TestFixture]
public class CrmAnalyticsServiceTests
{
    [Test]
    public async Task GetSummaryAsync_ShouldReturnCorrectLeadStatusCounts()
    {
        await using var context = CrmAnalyticsTestFactory.CreateContext();
        context.Leads.AddRange(
            CrmAnalyticsTestFactory.Lead("New", "New"),
            CrmAnalyticsTestFactory.Lead("Contacted", "Contacted"),
            CrmAnalyticsTestFactory.Lead("Qualified", "Qualified"),
            CrmAnalyticsTestFactory.Lead("Proposal", "Proposal"),
            CrmAnalyticsTestFactory.Lead("Won", "Won"),
            CrmAnalyticsTestFactory.Lead("Lost", "Lost"));
        await context.SaveChangesAsync();

        var result = await CrmAnalyticsTestFactory.CreateService(context).GetSummaryAsync(new CrmAnalyticsQueryDto());

        result.TotalLeads.Should().Be(6);
        result.NewLeads.Should().Be(1);
        result.ContactedLeads.Should().Be(1);
        result.QualifiedLeads.Should().Be(1);
        result.ProposalLeads.Should().Be(1);
        result.WonLeads.Should().Be(1);
        result.LostLeads.Should().Be(1);
    }

    [Test]
    public async Task GetSummaryAsync_ShouldReturnLeadTemperatureCounts()
    {
        await using var context = CrmAnalyticsTestFactory.CreateContext();
        context.Leads.AddRange(
            CrmAnalyticsTestFactory.Lead("Cold", score: 10),
            CrmAnalyticsTestFactory.Lead("Warm", score: 45),
            CrmAnalyticsTestFactory.Lead("Hot", score: 80));
        await context.SaveChangesAsync();

        var result = await CrmAnalyticsTestFactory.CreateService(context).GetSummaryAsync(new CrmAnalyticsQueryDto());

        result.ColdLeads.Should().Be(1);
        result.WarmLeads.Should().Be(1);
        result.HotLeads.Should().Be(1);
    }

    [Test]
    public async Task GetSummaryAsync_ShouldReturnCustomerCountsAndConversionRates()
    {
        await using var context = CrmAnalyticsTestFactory.CreateContext();
        var convertedLead = CrmAnalyticsTestFactory.Lead("Won", "Won");
        context.Leads.AddRange(convertedLead, CrmAnalyticsTestFactory.Lead("Lost", "Lost"), CrmAnalyticsTestFactory.Lead("New", "New"));
        context.Customers.AddRange(
            CrmAnalyticsTestFactory.Customer("Direct"),
            CrmAnalyticsTestFactory.Customer("Converted", convertedFromLeadId: convertedLead.Id));
        await context.SaveChangesAsync();

        var result = await CrmAnalyticsTestFactory.CreateService(context).GetSummaryAsync(new CrmAnalyticsQueryDto());

        result.TotalCustomers.Should().Be(2);
        result.CustomersFromLeads.Should().Be(1);
        result.DirectCustomers.Should().Be(1);
        result.LeadToWonConversionRate.Should().Be(33.33m);
        result.LeadToCustomerConversionRate.Should().Be(33.33m);
    }

    [Test]
    public async Task GetSummaryAsync_NoLeads_ShouldReturnZeroRates()
    {
        await using var context = CrmAnalyticsTestFactory.CreateContext();

        var result = await CrmAnalyticsTestFactory.CreateService(context).GetSummaryAsync(new CrmAnalyticsQueryDto());

        result.TotalLeads.Should().Be(0);
        result.LeadToWonConversionRate.Should().Be(0);
        result.LeadToCustomerConversionRate.Should().Be(0);
    }

    [Test]
    public async Task GetLeadsByStatusAsync_ShouldReturnAllStatusesIncludingZeroAndPercentages()
    {
        await using var context = CrmAnalyticsTestFactory.CreateContext();
        context.Leads.AddRange(
            CrmAnalyticsTestFactory.Lead("A", "New"),
            CrmAnalyticsTestFactory.Lead("B", "New"),
            CrmAnalyticsTestFactory.Lead("C", "Won"));
        await context.SaveChangesAsync();

        var result = await CrmAnalyticsTestFactory.CreateService(context).GetLeadsByStatusAsync(new CrmAnalyticsQueryDto());

        result.Should().HaveCount(6);
        result.Single(x => x.Label == "New").Count.Should().Be(2);
        result.Single(x => x.Label == "New").Percentage.Should().Be(66.67m);
        result.Single(x => x.Label == "Contacted").Count.Should().Be(0);
    }

    [Test]
    public async Task GetLeadsBySourceAsync_ShouldGroupUnknownAndSortByCountDesc()
    {
        await using var context = CrmAnalyticsTestFactory.CreateContext();
        context.Leads.AddRange(
            CrmAnalyticsTestFactory.Lead("Facebook 1", sourceChannel: "Facebook"),
            CrmAnalyticsTestFactory.Lead("Facebook 2", sourceChannel: "Facebook"),
            CrmAnalyticsTestFactory.Lead("Zalo", sourceChannel: "Zalo"),
            CrmAnalyticsTestFactory.Lead("Unknown"));
        await context.SaveChangesAsync();

        var result = await CrmAnalyticsTestFactory.CreateService(context).GetLeadsBySourceAsync(new CrmAnalyticsQueryDto());

        result.First().Label.Should().Be("Facebook");
        result.First().Count.Should().Be(2);
        result.Single(x => x.Label == "Zalo").Count.Should().Be(1);
        result.Single(x => x.Label == "Unknown").Count.Should().Be(1);
    }

    [Test]
    public async Task GetConversionStatsAsync_ShouldReturnCorrectRates()
    {
        await using var context = CrmAnalyticsTestFactory.CreateContext();
        var convertedLead = CrmAnalyticsTestFactory.Lead("Won", "Won");
        context.Leads.AddRange(
            convertedLead,
            CrmAnalyticsTestFactory.Lead("Lost", "Lost"),
            CrmAnalyticsTestFactory.Lead("New", "New"),
            CrmAnalyticsTestFactory.Lead("Contacted", "Contacted"));
        context.Customers.AddRange(
            CrmAnalyticsTestFactory.Customer("Converted", convertedFromLeadId: convertedLead.Id),
            CrmAnalyticsTestFactory.Customer("Direct"));
        await context.SaveChangesAsync();

        var result = await CrmAnalyticsTestFactory.CreateService(context).GetConversionStatsAsync(new CrmAnalyticsQueryDto());

        result.TotalLeads.Should().Be(4);
        result.LeadToWonConversionRate.Should().Be(25);
        result.LeadToCustomerConversionRate.Should().Be(25);
        result.LostRate.Should().Be(25);
    }

    [Test]
    public async Task GetConversionStatsAsync_NoLeads_ShouldReturnZeroRates()
    {
        await using var context = CrmAnalyticsTestFactory.CreateContext();

        var result = await CrmAnalyticsTestFactory.CreateService(context).GetConversionStatsAsync(new CrmAnalyticsQueryDto());

        result.LeadToWonConversionRate.Should().Be(0);
        result.LeadToCustomerConversionRate.Should().Be(0);
        result.LostRate.Should().Be(0);
    }

    [Test]
    public async Task GetFollowUpStatsAsync_ShouldCountFollowUpBucketsAndIgnoreWonLost()
    {
        await using var context = CrmAnalyticsTestFactory.CreateContext();
        var now = DateTime.UtcNow;
        context.Leads.AddRange(
            CrmAnalyticsTestFactory.Lead("Overdue", followUpAt: now.AddDays(-1)),
            CrmAnalyticsTestFactory.Lead("Due Today", followUpAt: now.Date.AddHours(12)),
            CrmAnalyticsTestFactory.Lead("Upcoming", followUpAt: now.AddDays(3)),
            CrmAnalyticsTestFactory.Lead("No Follow Up"),
            CrmAnalyticsTestFactory.Lead("Won Ignored", "Won", followUpAt: now.AddDays(-1)),
            CrmAnalyticsTestFactory.Lead("Lost Ignored", "Lost", followUpAt: now.AddDays(1)));
        await context.SaveChangesAsync();

        var result = await CrmAnalyticsTestFactory.CreateService(context).GetFollowUpStatsAsync(new CrmAnalyticsQueryDto());

        result.TotalLeadsWithFollowUp.Should().Be(3);
        result.OverdueFollowUps.Should().Be(1);
        result.DueTodayFollowUps.Should().Be(1);
        result.UpcomingFollowUps.Should().Be(2);
        result.NoFollowUpLeads.Should().Be(1);
    }

    [Test]
    public async Task GetCustomerStatsAsync_ShouldReturnTotalsSourceGroupsAndRate()
    {
        await using var context = CrmAnalyticsTestFactory.CreateContext();
        var leadId = Guid.NewGuid();
        context.Customers.AddRange(
            CrmAnalyticsTestFactory.Customer("Direct Website", "Website"),
            CrmAnalyticsTestFactory.Customer("Converted Website", "Website", leadId),
            CrmAnalyticsTestFactory.Customer("Unknown"));
        await context.SaveChangesAsync();

        var result = await CrmAnalyticsTestFactory.CreateService(context).GetCustomerStatsAsync(new CrmAnalyticsQueryDto());

        result.TotalCustomers.Should().Be(3);
        result.CustomersFromLeads.Should().Be(1);
        result.DirectCustomers.Should().Be(2);
        result.CustomersFromLeadsRate.Should().Be(33.33m);
        result.CustomersBySource.Single(x => x.Label == "Website").Count.Should().Be(2);
        result.CustomersBySource.Single(x => x.Label == "Unknown").Count.Should().Be(1);
    }

    [Test]
    public async Task GetMonthlyTrendAsync_ShouldReturn12MonthsAndCounts()
    {
        await using var context = CrmAnalyticsTestFactory.CreateContext();
        var year = DateTime.UtcNow.Year;
        var lead = CrmAnalyticsTestFactory.Lead("January Lead");
        var customer = CrmAnalyticsTestFactory.Customer("January Customer", convertedFromLeadId: lead.Id);
        context.Leads.Add(lead);
        context.Customers.Add(customer);
        await context.SaveChangesAsync();
        lead.CreatedAt = new DateTime(year, 1, 10);
        customer.CreatedAt = new DateTime(year, 1, 11);
        await context.SaveChangesAsync();

        var result = await CrmAnalyticsTestFactory.CreateService(context).GetMonthlyTrendAsync(year);

        result.Should().HaveCount(12);
        result[0].Month.Should().Be("Jan");
        result[0].Leads.Should().Be(1);
        result[0].Customers.Should().Be(1);
        result[0].ConvertedCustomers.Should().Be(1);
        result[1].Leads.Should().Be(0);
    }

    [Test]
    public async Task GetMonthlyTrendAsync_NullYear_ShouldUseCurrentYear()
    {
        await using var context = CrmAnalyticsTestFactory.CreateContext();

        var result = await CrmAnalyticsTestFactory.CreateService(context).GetMonthlyTrendAsync(null);

        result.Should().HaveCount(12);
    }

    [Test]
    public async Task Analytics_ShouldFilterByFromDateToDate()
    {
        await using var context = CrmAnalyticsTestFactory.CreateContext();
        var inside = CrmAnalyticsTestFactory.Lead("Inside");
        var outside = CrmAnalyticsTestFactory.Lead("Outside");
        context.Leads.AddRange(inside, outside);
        await context.SaveChangesAsync();
        inside.CreatedAt = new DateTime(2026, 6, 10);
        outside.CreatedAt = new DateTime(2026, 5, 10);
        await context.SaveChangesAsync();

        var result = await CrmAnalyticsTestFactory.CreateService(context).GetSummaryAsync(new CrmAnalyticsQueryDto
        {
            FromDate = new DateTime(2026, 6, 1),
            ToDate = new DateTime(2026, 6, 30)
        });

        result.TotalLeads.Should().Be(1);
    }

    [Test]
    public async Task Analytics_ShouldFilterByAssignedSourceAndStatus()
    {
        await using var context = CrmAnalyticsTestFactory.CreateContext();
        var assignedTo = Guid.NewGuid();
        context.Leads.AddRange(
            CrmAnalyticsTestFactory.Lead("Match", "Won", sourceChannel: "Facebook", assignedToId: assignedTo),
            CrmAnalyticsTestFactory.Lead("Wrong Status", "New", sourceChannel: "Facebook", assignedToId: assignedTo),
            CrmAnalyticsTestFactory.Lead("Wrong Source", "Won", sourceChannel: "Zalo", assignedToId: assignedTo),
            CrmAnalyticsTestFactory.Lead("Wrong User", "Won", sourceChannel: "Facebook", assignedToId: Guid.NewGuid()));
        context.Customers.AddRange(
            CrmAnalyticsTestFactory.Customer("Customer Match", "Facebook", assignedToId: assignedTo),
            CrmAnalyticsTestFactory.Customer("Customer Wrong Source", "Zalo", assignedToId: assignedTo));
        await context.SaveChangesAsync();

        var query = new CrmAnalyticsQueryDto
        {
            AssignedToId = assignedTo,
            SourceChannel = "Facebook",
            Status = "Won"
        };
        var summary = await CrmAnalyticsTestFactory.CreateService(context).GetSummaryAsync(query);
        var customers = await CrmAnalyticsTestFactory.CreateService(context).GetCustomerStatsAsync(query);

        summary.TotalLeads.Should().Be(1);
        customers.TotalCustomers.Should().Be(1);
    }
}
