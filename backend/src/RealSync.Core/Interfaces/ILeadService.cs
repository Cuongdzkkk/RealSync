using RealSync.Shared.DTOs.Requests.Leads;
using RealSync.Shared.DTOs.Responses.Customers;
using RealSync.Shared.DTOs.Responses.Leads;

namespace RealSync.Core.Interfaces;

public interface ILeadService
{
    Task<(IReadOnlyList<LeadListItemDto> Items, int TotalCount)> GetLeadsAsync(LeadQueryDto query);
    Task<LeadDetailDto> GetLeadByIdAsync(Guid id);
    Task<LeadResponseDto> CreateLeadAsync(LeadCreateDto dto);
    Task<LeadResponseDto> UpdateLeadAsync(Guid id, LeadUpdateDto dto);
    Task DeleteLeadAsync(Guid id);
    Task<LeadResponseDto> UpdateStatusAsync(Guid id, LeadStatusUpdateDto dto);
    Task<LeadResponseDto> AssignLeadAsync(Guid id, LeadAssignDto dto);
    Task<LeadActivityDto> AddActivityAsync(Guid id, LeadActivityCreateDto dto);
    Task<IReadOnlyList<LeadActivityDto>> GetActivitiesAsync(Guid id);
    Task<LeadResponseDto> SetFollowUpAsync(Guid id, LeadFollowUpDto dto);
    Task<CustomerResponseDto> ConvertToCustomerAsync(Guid id, LeadConvertToCustomerDto dto);
}
