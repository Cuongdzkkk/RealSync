using RealSync.Shared.DTOs.Requests.Customers;
using RealSync.Shared.DTOs.Responses.Customers;

namespace RealSync.Core.Interfaces;

public interface ICustomerService
{
    Task<(IReadOnlyList<CustomerListItemDto> Items, int TotalCount)> GetCustomersAsync(CustomerQueryDto query);
    Task<CustomerDetailDto> GetCustomerByIdAsync(Guid id);
    Task<CustomerResponseDto> CreateCustomerAsync(CustomerCreateDto dto);
    Task<CustomerResponseDto> UpdateCustomerAsync(Guid id, CustomerUpdateDto dto);
    Task DeleteCustomerAsync(Guid id);
    Task<IReadOnlyList<CustomerActivityLogDto>> GetCustomerActivitiesAsync(Guid id);
}
