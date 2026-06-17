using Microsoft.EntityFrameworkCore;
using RealSync.Core.Entities;
using RealSync.Core.Interfaces;
using RealSync.Data.Context;
using RealSync.Shared.DTOs.Requests.Customers;

namespace RealSync.Data.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly RealSyncDbContext _context;

    public CustomerRepository(RealSyncDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<Customer> Items, int TotalCount)> GetPagedAsync(CustomerQueryDto query)
    {
        IQueryable<Customer> customers = _context.Customers
            .AsNoTracking()
            .Include(c => c.AssignedTo)
            .Include(c => c.ConvertedFromLead);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var keyword = query.Search.Trim();
            customers = customers.Where(c =>
                c.FullName.Contains(keyword) ||
                (c.Phone != null && c.Phone.Contains(keyword)) ||
                (c.Email != null && c.Email.Contains(keyword)) ||
                (c.Company != null && c.Company.Contains(keyword)) ||
                (c.Address != null && c.Address.Contains(keyword)) ||
                (c.Notes != null && c.Notes.Contains(keyword)) ||
                (c.Source != null && c.Source.Contains(keyword)));
        }

        if (!string.IsNullOrWhiteSpace(query.Source))
            customers = customers.Where(c => c.Source == query.Source.Trim());

        if (query.AssignedToId.HasValue)
            customers = customers.Where(c => c.AssignedToId == query.AssignedToId.Value);

        if (query.ConvertedFromLeadId.HasValue)
            customers = customers.Where(c => c.ConvertedFromLeadId == query.ConvertedFromLeadId.Value);

        if (query.ConvertedFromLead.HasValue)
        {
            customers = query.ConvertedFromLead.Value
                ? customers.Where(c => c.ConvertedFromLeadId != null)
                : customers.Where(c => c.ConvertedFromLeadId == null);
        }

        if (query.FromDate.HasValue)
            customers = customers.Where(c => c.CreatedAt >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            customers = customers.Where(c => c.CreatedAt <= query.ToDate.Value);

        var totalCount = await customers.CountAsync();
        customers = ApplySorting(customers, query.SortBy, query.SortDirection);

        var items = await customers
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Customer?> GetByIdAsync(Guid id)
    {
        return await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Customer?> GetDetailByIdAsync(Guid id)
    {
        return await _context.Customers
            .Include(c => c.AssignedTo)
            .Include(c => c.ConvertedFromLead)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Customer?> GetByConvertedLeadIdAsync(Guid leadId)
    {
        return await _context.Customers.FirstOrDefaultAsync(c => c.ConvertedFromLeadId == leadId);
    }

    public async Task<Customer> CreateAsync(Customer customer)
    {
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task UpdateAsync(Customer customer)
    {
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Customer customer)
    {
        customer.IsDeleted = true;
        customer.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Customers.AnyAsync(c => c.Id == id);
    }

    public async Task<bool> ExistsByConvertedLeadIdAsync(Guid leadId)
    {
        return await _context.Customers.AnyAsync(c => c.ConvertedFromLeadId == leadId);
    }

    public async Task<IReadOnlyList<ActivityLog>> GetActivityLogsAsync(Guid customerId)
    {
        return await _context.ActivityLogs
            .AsNoTracking()
            .Include(a => a.User)
            .Where(a => a.EntityType == "Customer" && a.EntityId == customerId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    private static IQueryable<Customer> ApplySorting(
        IQueryable<Customer> query,
        string? sortBy,
        string? sortDirection)
    {
        var ascending = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);

        return sortBy?.ToLowerInvariant() switch
        {
            "fullname" or "fullName" => ascending ? query.OrderBy(c => c.FullName) : query.OrderByDescending(c => c.FullName),
            "company" => ascending ? query.OrderBy(c => c.Company) : query.OrderByDescending(c => c.Company),
            "source" => ascending ? query.OrderBy(c => c.Source) : query.OrderByDescending(c => c.Source),
            "createdat" or "createdAt" => ascending ? query.OrderBy(c => c.CreatedAt) : query.OrderByDescending(c => c.CreatedAt),
            _ => query.OrderByDescending(c => c.CreatedAt)
        };
    }
}
