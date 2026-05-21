using System.Linq.Expressions;
using RealSync.Core.Entities;
using RealSync.Shared.DTOs.Requests;

namespace RealSync.Core.Interfaces;

/// <summary>
/// Generic repository interface cho tất cả entities.
/// Cung cấp các thao tác CRUD cơ bản + phân trang.
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        PaginationRequest pagination,
        Expression<Func<T, bool>>? filter = null);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);  // Soft delete
    Task<bool> ExistsAsync(Guid id);
}
