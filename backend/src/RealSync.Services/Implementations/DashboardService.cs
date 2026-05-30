using Microsoft.EntityFrameworkCore;
using RealSync.Core.Interfaces;
using RealSync.Data.Context;
using RealSync.Shared.DTOs.Responses.Dashboard;

namespace RealSync.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly RealSyncDbContext _context;

    public DashboardService(RealSyncDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatisticsResponse> GetStatisticsAsync()
    {
        var now = DateTime.UtcNow;
        var startOfThisMonth = new DateTime(now.Year, now.Month, 1);
        var startOfLastMonth = startOfThisMonth.AddMonths(-1);

        var totalLeads = await _context.Leads.CountAsync();
        var leadsThisMonth = await _context.Leads.CountAsync(l => l.CreatedAt >= startOfThisMonth);
        var leadsLastMonth = await _context.Leads.CountAsync(l => l.CreatedAt >= startOfLastMonth && l.CreatedAt < startOfThisMonth);

        var totalProperties = await _context.Properties.CountAsync();
        var propsThisMonth = await _context.Properties.CountAsync(p => p.CreatedAt >= startOfThisMonth);
        var propsLastMonth = await _context.Properties.CountAsync(p => p.CreatedAt >= startOfLastMonth && p.CreatedAt < startOfThisMonth);

        var totalCustomers = await _context.Customers.CountAsync();
        var custsThisMonth = await _context.Customers.CountAsync(c => c.CreatedAt >= startOfThisMonth);
        var custsLastMonth = await _context.Customers.CountAsync(c => c.CreatedAt >= startOfLastMonth && c.CreatedAt < startOfThisMonth);

        return new DashboardStatisticsResponse
        {
            TotalLeads = totalLeads,
            TotalProperties = totalProperties,
            TotalCustomers = totalCustomers,
            LeadsGrowthRate = CalculateGrowth(leadsThisMonth, leadsLastMonth),
            PropertiesGrowthRate = CalculateGrowth(propsThisMonth, propsLastMonth),
            CustomersGrowthRate = CalculateGrowth(custsThisMonth, custsLastMonth)
        };
    }

    public async Task<IEnumerable<MonthlyStatItem>> GetMonthlyLeadsAsync()
    {
        var now = DateTime.UtcNow;
        var startDate = new DateTime(now.Year, 1, 1); // Start of current year

        var data = await _context.Leads
            .Where(l => l.CreatedAt >= startDate)
            .GroupBy(l => l.CreatedAt.Month)
            .Select(g => new { Month = g.Key, Count = g.Count() })
            .ToListAsync();

        var result = new List<MonthlyStatItem>();
        for (int i = 1; i <= 12; i++)
        {
            var item = data.FirstOrDefault(d => d.Month == i);
            result.Add(new MonthlyStatItem
            {
                Month = new DateTime(now.Year, i, 1).ToString("MMM"), // e.g. "Jan", "Feb"
                Count = item?.Count ?? 0
            });
        }

        return result;
    }

    public async Task<IEnumerable<PropertyStatusStatItem>> GetPropertiesByStatusAsync()
    {
        return await _context.Properties
            .GroupBy(p => p.Status)
            .Select(g => new PropertyStatusStatItem
            {
                Status = g.Key,
                Count = g.Count()
            })
            .ToListAsync();
    }

    private decimal CalculateGrowth(int current, int previous)
    {
        if (previous == 0) return current > 0 ? 100 : 0;
        return Math.Round((decimal)(current - previous) / previous * 100, 2);
    }
}
