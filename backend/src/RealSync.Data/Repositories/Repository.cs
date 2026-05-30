using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RealSync.Core.Entities;
using RealSync.Core.Interfaces;
using RealSync.Data.Context;
using RealSync.Shared.DTOs.Requests;

namespace RealSync.Data.Repositories;

/// <summary>
/// Generic repository implementation.
/// Soft delete được tự động áp dụng qua global query filter.
/// </summary>
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly RealSyncDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(RealSyncDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking().ToListAsync();
    }

    public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        PaginationRequest pagination,
        Expression<Func<T, bool>>? filter = null)
    {
        IQueryable<T> query = _dbSet.AsNoTracking();

        if (filter != null)
            query = query.Where(filter);

        var totalCount = await query.CountAsync();

        // Default sort by CreatedAt desc
        query = pagination.SortDirection?.ToLower() == "asc"
            ? query.OrderBy(e => e.CreatedAt)
            : query.OrderByDescending(e => e.CreatedAt);

        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null)
        {
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public virtual async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.AnyAsync(e => e.Id == id);
    }
}
