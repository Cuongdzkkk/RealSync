using Microsoft.AspNetCore.Mvc;
using RealSync.Api.Filters;
using RealSync.Core.Interfaces;
using RealSync.Shared.DTOs.Requests.Customers;
using RealSync.Shared.DTOs.Responses.Customers;

namespace RealSync.Api.Controllers;

[Route("api/customers")]
[Route("api/v1/customers")]
public class CustomersController : BaseController
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    [RequirePermission("customers.read")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<IEnumerable<CustomerListItemDto>>), 200)]
    public async Task<IActionResult> GetCustomers([FromQuery] CustomerQueryDto query)
    {
        var (items, totalCount) = await _customerService.GetCustomersAsync(query);
        return PagedResponse(items, query.Page, query.PageSize, totalCount);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("customers.read")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<CustomerDetailDto>), 200)]
    public async Task<IActionResult> GetCustomer(Guid id)
    {
        var result = await _customerService.GetCustomerByIdAsync(id);
        return OkResponse(result);
    }

    [HttpPost]
    [RequirePermission("customers.create")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<CustomerResponseDto>), 201)]
    public async Task<IActionResult> CreateCustomer([FromBody] CustomerCreateDto request)
    {
        var result = await _customerService.CreateCustomerAsync(request);
        return CreatedResponse(result, "Tạo khách hàng thành công");
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("customers.update")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<CustomerResponseDto>), 200)]
    public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] CustomerUpdateDto request)
    {
        var result = await _customerService.UpdateCustomerAsync(id, request);
        return OkResponse(result, "Cập nhật khách hàng thành công");
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("customers.delete")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteCustomer(Guid id)
    {
        await _customerService.DeleteCustomerAsync(id);
        return NoContent();
    }

    [HttpGet("{id:guid}/activities")]
    [RequirePermission("customers.read")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<IReadOnlyList<CustomerActivityLogDto>>), 200)]
    public async Task<IActionResult> GetActivities(Guid id)
    {
        var result = await _customerService.GetCustomerActivitiesAsync(id);
        return OkResponse(result);
    }
}
