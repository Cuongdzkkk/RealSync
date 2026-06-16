using RealSync.Shared.DTOs.Requests.Leads;
using RealSync.Shared.DTOs.Responses.Leads;

namespace RealSync.Core.Interfaces;

public interface ILeadService
{
    Task<(IReadOnlyList<LeadListItemDto> Items, int TotalCount)> GetLeadsAsync(LeadQueryDto query);
    Task<LeadDetailDto> GetLeadByIdAsync(Guid id);
    Task<LeadResponseDto> CreateLeadAsync(LeadCreateDto dto);
    Task<LeadResponseDto> UpdateLeadAsync(Guid id, LeadUpdateDto dto);
    Task DeleteLeadAsync(Guid id);
}
