using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RealSync.Core.Entities;
using RealSync.Core.Interfaces;
using RealSync.Data.Context;
using RealSync.Shared.DTOs.Requests.CrmAnalytics;
using RealSync.Shared.DTOs.Responses.CrmAnalytics;

namespace RealSync.Services.Implementations;

public class CrmAnalyticsService : ICrmAnalyticsService
{
    private static readonly string[] LeadStatuses =
    {
        "New",
        "Contacted",
        "Qualified",
        "Proposal",
        "Won",
        "Lost"
    };

    private readonly RealSyncDbContext _context;
    private readonly ILogger<CrmAnalyticsService> _logger;

    public CrmAnalyticsService(
        RealSyncDbContext context,
        ILogger<CrmAnalyticsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CrmAnalyticsSummaryDto> GetSummaryAsync(CrmAnalyticsQueryDto query)
    {
        _logger.LogDebug("Generating CRM analytics summary");

        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var tomorrowStart = todayStart.AddDays(1);
        var nextWeek = now.AddDays(7);

        var leads = ApplyLeadFilters(_context.Leads.AsNoTracking(), query);
        var customers = ApplyCustomerFilters(_context.Customers.AsNoTracking(), query);
        var activePipelineLeads = leads.Where(l => l.Status != "Won" && l.Status != "Lost");

        var totalLeads = await leads.CountAsync();
        var wonLeads = await leads.CountAsync(l => l.Status == "Won");
        var customersFromLeads = await customers.CountAsync(c => c.ConvertedFromLeadId != null);

        return new CrmAnalyticsSummaryDto
        {
            TotalLeads = totalLeads,
            NewLeads = await leads.CountAsync(l => l.Status == "New"),
            ContactedLeads = await leads.CountAsync(l => l.Status == "Contacted"),
            QualifiedLeads = await leads.CountAsync(l => l.Status == "Qualified"),
            ProposalLeads = await leads.CountAsync(l => l.Status == "Proposal"),
            WonLeads = wonLeads,
            LostLeads = await leads.CountAsync(l => l.Status == "Lost"),
            HotLeads = await leads.CountAsync(l => l.Score >= 70),
            WarmLeads = await leads.CountAsync(l => l.Score >= 40 && l.Score < 70),
            ColdLeads = await leads.CountAsync(l => l.Score < 40),
            TotalCustomers = await customers.CountAsync(),
            CustomersFromLeads = customersFromLeads,
            DirectCustomers = await customers.CountAsync(c => c.ConvertedFromLeadId == null),
            TotalLeadActivities = await _context.LeadActivities.AsNoTracking()
                .CountAsync(a => leads.Select(l => l.Id).Contains(a.LeadId)),
            OverdueFollowUps = await activePipelineLeads.CountAsync(l => l.NextFollowUpAt != null && l.NextFollowUpAt < now),
            DueTodayFollowUps = await activePipelineLeads.CountAsync(l => l.NextFollowUpAt >= todayStart && l.NextFollowUpAt < tomorrowStart),
            UpcomingFollowUps = await activePipelineLeads.CountAsync(l => l.NextFollowUpAt > now && l.NextFollowUpAt <= nextWeek),
            LeadToWonConversionRate = CalculateRate(wonLeads, totalLeads),
            LeadToCustomerConversionRate = CalculateRate(customersFromLeads, totalLeads),
            GeneratedAt = now
        };
    }

    public async Task<IReadOnlyList<CrmCountByLabelDto>> GetLeadsByStatusAsync(CrmAnalyticsQueryDto query)
    {
        var leads = ApplyLeadFilters(_context.Leads.AsNoTracking(), query);
        var total = await leads.CountAsync();
        var counts = await leads
            .GroupBy(l => l.Status)
            .Select(g => new { Label = g.Key, Count = g.Count() })
            .ToListAsync();

        return LeadStatuses
            .Select(status =>
            {
                var count = counts.FirstOrDefault(c => c.Label == status)?.Count ?? 0;
                return new CrmCountByLabelDto
                {
                    Label = status,
                    Count = count,
                    Percentage = CalculatePercentage(count, total)
                };
            })
            .ToList();
    }

    public async Task<IReadOnlyList<CrmCountByLabelDto>> GetLeadsBySourceAsync(CrmAnalyticsQueryDto query)
    {
        var leads = ApplyLeadFilters(_context.Leads.AsNoTracking(), query);
        var total = await leads.CountAsync();

        var counts = await leads
            .GroupBy(l => l.SourceChannel == null || l.SourceChannel == string.Empty ? "Unknown" : l.SourceChannel)
            .Select(g => new CrmCountByLabelDto
            {
                Label = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.Label)
            .ToListAsync();

        foreach (var item in counts)
            item.Percentage = CalculatePercentage(item.Count, total);

        return counts;
    }

    public async Task<CrmConversionStatsDto> GetConversionStatsAsync(CrmAnalyticsQueryDto query)
    {
        var leads = ApplyLeadFilters(_context.Leads.AsNoTracking(), query);
        var customers = ApplyCustomerFilters(_context.Customers.AsNoTracking(), query);
        var totalLeads = await leads.CountAsync();
        var wonLeads = await leads.CountAsync(l => l.Status == "Won");
        var lostLeads = await leads.CountAsync(l => l.Status == "Lost");
        var customersFromLeads = await customers.CountAsync(c => c.ConvertedFromLeadId != null);
        var directCustomers = await customers.CountAsync(c => c.ConvertedFromLeadId == null);

        return new CrmConversionStatsDto
        {
            TotalLeads = totalLeads,
            WonLeads = wonLeads,
            LostLeads = lostLeads,
            CustomersFromLeads = customersFromLeads,
            DirectCustomers = directCustomers,
            LeadToWonConversionRate = CalculateRate(wonLeads, totalLeads),
            LeadToCustomerConversionRate = CalculateRate(customersFromLeads, totalLeads),
            LostRate = CalculateRate(lostLeads, totalLeads)
        };
    }

    public async Task<CrmFollowUpStatsDto> GetFollowUpStatsAsync(CrmAnalyticsQueryDto query)
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var tomorrowStart = todayStart.AddDays(1);
        var nextWeek = now.AddDays(7);

        var activePipelineLeads = ApplyLeadFilters(_context.Leads.AsNoTracking(), query)
            .Where(l => l.Status != "Won" && l.Status != "Lost");

        return new CrmFollowUpStatsDto
        {
            TotalLeadsWithFollowUp = await activePipelineLeads.CountAsync(l => l.NextFollowUpAt != null),
            OverdueFollowUps = await activePipelineLeads.CountAsync(l => l.NextFollowUpAt != null && l.NextFollowUpAt < now),
            DueTodayFollowUps = await activePipelineLeads.CountAsync(l => l.NextFollowUpAt >= todayStart && l.NextFollowUpAt < tomorrowStart),
            UpcomingFollowUps = await activePipelineLeads.CountAsync(l => l.NextFollowUpAt > now && l.NextFollowUpAt <= nextWeek),
            NoFollowUpLeads = await activePipelineLeads.CountAsync(l => l.NextFollowUpAt == null)
        };
    }

    public async Task<CrmCustomerStatsDto> GetCustomerStatsAsync(CrmAnalyticsQueryDto query)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var customers = ApplyCustomerFilters(_context.Customers.AsNoTracking(), query);
        var totalCustomers = await customers.CountAsync();
        var customersFromLeads = await customers.CountAsync(c => c.ConvertedFromLeadId != null);

        var sourceCounts = await customers
            .GroupBy(c => c.Source == null || c.Source == string.Empty ? "Unknown" : c.Source)
            .Select(g => new CrmCountByLabelDto
            {
                Label = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.Label)
            .ToListAsync();

        foreach (var item in sourceCounts)
            item.Percentage = CalculatePercentage(item.Count, totalCustomers);

        return new CrmCustomerStatsDto
        {
            TotalCustomers = totalCustomers,
            CustomersFromLeads = customersFromLeads,
            DirectCustomers = await customers.CountAsync(c => c.ConvertedFromLeadId == null),
            NewCustomersThisMonth = await customers.CountAsync(c => c.CreatedAt >= startOfMonth),
            CustomersFromLeadsRate = CalculateRate(customersFromLeads, totalCustomers),
            CustomersBySource = sourceCounts
        };
    }

    public async Task<IReadOnlyList<CrmMonthlyTrendDto>> GetMonthlyTrendAsync(int? year)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var start = new DateTime(targetYear, 1, 1);
        var end = start.AddYears(1);

        var leadCounts = await _context.Leads
            .AsNoTracking()
            .Where(l => l.CreatedAt >= start && l.CreatedAt < end)
            .GroupBy(l => l.CreatedAt.Month)
            .Select(g => new { Month = g.Key, Count = g.Count() })
            .ToListAsync();

        var customerCounts = await _context.Customers
            .AsNoTracking()
            .Where(c => c.CreatedAt >= start && c.CreatedAt < end)
            .GroupBy(c => c.CreatedAt.Month)
            .Select(g => new
            {
                Month = g.Key,
                Count = g.Count(),
                ConvertedCount = g.Count(c => c.ConvertedFromLeadId != null)
            })
            .ToListAsync();

        var result = new List<CrmMonthlyTrendDto>();
        for (var month = 1; month <= 12; month++)
        {
            var date = new DateTime(targetYear, month, 1);
            var lead = leadCounts.FirstOrDefault(x => x.Month == month);
            var customer = customerCounts.FirstOrDefault(x => x.Month == month);

            result.Add(new CrmMonthlyTrendDto
            {
                Month = date.ToString("MMM", CultureInfo.InvariantCulture),
                Leads = lead?.Count ?? 0,
                Customers = customer?.Count ?? 0,
                ConvertedCustomers = customer?.ConvertedCount ?? 0
            });
        }

        return result;
    }

    private static IQueryable<Lead> ApplyLeadFilters(IQueryable<Lead> query, CrmAnalyticsQueryDto dto)
    {
        if (dto.FromDate.HasValue)
            query = query.Where(l => l.CreatedAt >= dto.FromDate.Value);

        if (dto.ToDate.HasValue)
            query = query.Where(l => l.CreatedAt <= dto.ToDate.Value);

        if (dto.AssignedToId.HasValue)
            query = query.Where(l => l.AssignedToId == dto.AssignedToId.Value);

        if (!string.IsNullOrWhiteSpace(dto.SourceChannel))
            query = query.Where(l => l.SourceChannel == dto.SourceChannel.Trim());

        if (!string.IsNullOrWhiteSpace(dto.Status))
            query = query.Where(l => l.Status == dto.Status.Trim());

        return query;
    }

    private static IQueryable<Customer> ApplyCustomerFilters(IQueryable<Customer> query, CrmAnalyticsQueryDto dto)
    {
        if (dto.FromDate.HasValue)
            query = query.Where(c => c.CreatedAt >= dto.FromDate.Value);

        if (dto.ToDate.HasValue)
            query = query.Where(c => c.CreatedAt <= dto.ToDate.Value);

        if (dto.AssignedToId.HasValue)
            query = query.Where(c => c.AssignedToId == dto.AssignedToId.Value);

        if (!string.IsNullOrWhiteSpace(dto.SourceChannel))
            query = query.Where(c => c.Source == dto.SourceChannel.Trim());

        return query;
    }

    private static decimal CalculatePercentage(int count, int total)
    {
        return total == 0 ? 0 : Math.Round((decimal)count / total * 100, 2);
    }

    private static decimal CalculateRate(int numerator, int denominator)
    {
        return denominator == 0 ? 0 : Math.Round((decimal)numerator / denominator * 100, 2);
    }
}
