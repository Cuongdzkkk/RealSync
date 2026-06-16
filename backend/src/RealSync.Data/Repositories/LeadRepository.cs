using Microsoft.EntityFrameworkCore;
using RealSync.Core.Entities;
using RealSync.Core.Interfaces;
using RealSync.Data.Context;
using RealSync.Shared.DTOs.Requests.Leads;

namespace RealSync.Data.Repositories;

public class LeadRepository : ILeadRepository
{
    private readonly RealSyncDbContext _context;

    public LeadRepository(RealSyncDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<Lead> Items, int TotalCount)> GetPagedAsync(LeadQueryDto query)
    {
        IQueryable<Lead> leads = _context.Leads
            .AsNoTracking()
            .Include(l => l.AssignedTo)
            .Include(l => l.InterestedProperty);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var keyword = query.Search.Trim();
            leads = leads.Where(l =>
                l.FullName.Contains(keyword) ||
                (l.Phone != null && l.Phone.Contains(keyword)) ||
                (l.Email != null && l.Email.Contains(keyword)) ||
                (l.Requirements != null && l.Requirements.Contains(keyword)) ||
                (l.PreferredArea != null && l.PreferredArea.Contains(keyword)) ||
                (l.PreferredType != null && l.PreferredType.Contains(keyword)) ||
                (l.SourceChannel != null && l.SourceChannel.Contains(keyword)));
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
            leads = leads.Where(l => l.Status == query.Status.Trim());

        if (!string.IsNullOrWhiteSpace(query.Priority))
            leads = leads.Where(l => l.Priority == query.Priority.Trim());

        if (!string.IsNullOrWhiteSpace(query.SourceChannel))
            leads = leads.Where(l => l.SourceChannel == query.SourceChannel.Trim());

        if (query.AssignedToId.HasValue)
            leads = leads.Where(l => l.AssignedToId == query.AssignedToId.Value);

        if (query.InterestedPropertyId.HasValue)
            leads = leads.Where(l => l.InterestedPropertyId == query.InterestedPropertyId.Value);

        if (query.MinScore.HasValue)
            leads = leads.Where(l => l.Score >= query.MinScore.Value);

        if (query.MaxScore.HasValue)
            leads = leads.Where(l => l.Score <= query.MaxScore.Value);

        if (query.MinBudget.HasValue)
            leads = leads.Where(l => l.Budget >= query.MinBudget.Value);

        if (query.MaxBudget.HasValue)
            leads = leads.Where(l => l.Budget <= query.MaxBudget.Value);

        if (query.FromDate.HasValue)
            leads = leads.Where(l => l.CreatedAt >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            leads = leads.Where(l => l.CreatedAt <= query.ToDate.Value);

        var totalCount = await leads.CountAsync();
        leads = ApplySorting(leads, query.SortBy, query.SortDirection);

        var items = await leads
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Lead?> GetByIdAsync(Guid id)
    {
        return await _context.Leads.FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<Lead?> GetDetailByIdAsync(Guid id)
    {
        return await _context.Leads
            .Include(l => l.AssignedTo)
            .Include(l => l.InterestedProperty)
            .Include(l => l.Activities.OrderByDescending(a => a.CreatedAt))
                .ThenInclude(a => a.PerformedBy)
            .AsSplitQuery()
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<Lead> CreateAsync(Lead lead)
    {
        await _context.Leads.AddAsync(lead);
        await _context.SaveChangesAsync();
        return lead;
    }

    public async Task UpdateAsync(Lead lead)
    {
        _context.Leads.Update(lead);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Lead lead)
    {
        lead.IsDeleted = true;
        lead.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Leads.AnyAsync(l => l.Id == id);
    }

    private static IQueryable<Lead> ApplySorting(
        IQueryable<Lead> query,
        string? sortBy,
        string? sortDirection)
    {
        var ascending = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);

        return sortBy?.ToLowerInvariant() switch
        {
            "score" => ascending ? query.OrderBy(l => l.Score) : query.OrderByDescending(l => l.Score),
            "budget" => ascending ? query.OrderBy(l => l.Budget) : query.OrderByDescending(l => l.Budget),
            "fullname" or "fullName" => ascending ? query.OrderBy(l => l.FullName) : query.OrderByDescending(l => l.FullName),
            "nextfollowupat" or "nextFollowUpAt" => ascending ? query.OrderBy(l => l.NextFollowUpAt) : query.OrderByDescending(l => l.NextFollowUpAt),
            "createdat" or "createdAt" => ascending ? query.OrderBy(l => l.CreatedAt) : query.OrderByDescending(l => l.CreatedAt),
            _ => query.OrderByDescending(l => l.CreatedAt)
        };
    }
}
