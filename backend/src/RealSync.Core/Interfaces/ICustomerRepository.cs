using RealSync.Core.Entities;
using RealSync.Shared.DTOs.Requests.Customers;

namespace RealSync.Core.Interfaces;

public interface ICustomerRepository
{
    Task<(IReadOnlyList<Customer> Items, int TotalCount)> GetPagedAsync(CustomerQueryDto query);
    Task<Customer?> GetByIdAsync(Guid id);
    Task<Customer?> GetDetailByIdAsync(Guid id);
    Task<Customer?> GetByConvertedLeadIdAsync(Guid leadId);
    Task<Customer> CreateAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task DeleteAsync(Customer customer);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> ExistsByConvertedLeadIdAsync(Guid leadId);
    Task<IReadOnlyList<ActivityLog>> GetActivityLogsAsync(Guid customerId);
}
