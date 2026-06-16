using RealSync.Core.Entities;
using RealSync.Shared.DTOs.Requests.Leads;

namespace RealSync.Core.Interfaces;

public interface ILeadRepository
{
    Task<(IReadOnlyList<Lead> Items, int TotalCount)> GetPagedAsync(LeadQueryDto query);
    Task<Lead?> GetByIdAsync(Guid id);
    Task<Lead?> GetDetailByIdAsync(Guid id);
    Task<Lead> CreateAsync(Lead lead);
    Task UpdateAsync(Lead lead);
    Task DeleteAsync(Lead lead);
    Task<bool> ExistsAsync(Guid id);
}
