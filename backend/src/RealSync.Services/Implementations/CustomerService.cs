using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RealSync.Core.Entities;
using RealSync.Core.Enums;
using RealSync.Core.Interfaces;
using RealSync.Data.Context;
using RealSync.Shared.DTOs.Requests.Customers;
using RealSync.Shared.DTOs.Responses.Customers;
using RealSync.Shared.Exceptions;
using ValidationException = RealSync.Shared.Exceptions.ValidationException;

namespace RealSync.Services.Implementations;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly RealSyncDbContext _context;
    private readonly IActivityLogService _activityLogService;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        ICustomerRepository customerRepository,
        RealSyncDbContext context,
        IActivityLogService activityLogService,
        ILogger<CustomerService> logger)
    {
        _customerRepository = customerRepository;
        _context = context;
        _activityLogService = activityLogService;
        _logger = logger;
    }

    public async Task<(IReadOnlyList<CustomerListItemDto> Items, int TotalCount)> GetCustomersAsync(CustomerQueryDto query)
    {
        var (items, totalCount) = await _customerRepository.GetPagedAsync(query);
        return (items.Select(MapListItem).ToList(), totalCount);
    }

    public async Task<CustomerDetailDto> GetCustomerByIdAsync(Guid id)
    {
        var customer = await _customerRepository.GetDetailByIdAsync(id)
            ?? throw new NotFoundException("Customer", id);

        var logs = await _customerRepository.GetActivityLogsAsync(id);
        return MapDetail(customer, logs);
    }

    public async Task<CustomerResponseDto> CreateCustomerAsync(CustomerCreateDto dto)
    {
        var fullName = NormalizeRequired(dto.FullName, "fullName", "Tên khách hàng không được để trống.");
        var phone = NormalizeOptional(dto.Phone);
        var email = NormalizeOptional(dto.Email);

        if (phone == null && email == null)
            throw new ValidationException("contact", "Vui lòng nhập số điện thoại hoặc email.");

        if (dto.AssignedToId.HasValue)
            await EnsureActiveUserExistsAsync(dto.AssignedToId.Value);

        var customer = new Customer
        {
            FullName = fullName,
            Phone = phone,
            Email = email,
            Address = NormalizeOptional(dto.Address),
            Company = NormalizeOptional(dto.Company),
            Notes = NormalizeOptional(dto.Notes),
            Source = NormalizeOptional(dto.Source),
            AssignedToId = dto.AssignedToId
        };

        await _customerRepository.CreateAsync(customer);
        await TryLogAsync(customer.Id, ActivityType.Create, "Created customer", null, customer);

        return MapResponse(customer);
    }

    public async Task<CustomerResponseDto> UpdateCustomerAsync(Guid id, CustomerUpdateDto dto)
    {
        var customer = await _customerRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Customer", id);

        if (dto.FullName != null)
            customer.FullName = NormalizeRequired(dto.FullName, "fullName", "Tên khách hàng không được để trống.");

        if (dto.Phone != null)
            customer.Phone = NormalizeOptional(dto.Phone);

        if (dto.Email != null)
            customer.Email = NormalizeOptional(dto.Email);

        if (dto.Address != null)
            customer.Address = NormalizeOptional(dto.Address);

        if (dto.Company != null)
            customer.Company = NormalizeOptional(dto.Company);

        if (dto.Notes != null)
            customer.Notes = NormalizeOptional(dto.Notes);

        if (dto.Source != null)
            customer.Source = NormalizeOptional(dto.Source);

        if (dto.AssignedToId.HasValue)
        {
            await EnsureActiveUserExistsAsync(dto.AssignedToId.Value);
            customer.AssignedToId = dto.AssignedToId.Value;
        }

        if (string.IsNullOrWhiteSpace(customer.Phone) && string.IsNullOrWhiteSpace(customer.Email))
            throw new ValidationException("contact", "Khách hàng phải có số điện thoại hoặc email.");

        await _customerRepository.UpdateAsync(customer);
        await TryLogAsync(customer.Id, ActivityType.Update, "Updated customer", null, customer);

        return MapResponse(customer);
    }

    public async Task DeleteCustomerAsync(Guid id)
    {
        var customer = await _customerRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Customer", id);

        await _customerRepository.DeleteAsync(customer);
        await TryLogAsync(customer.Id, ActivityType.Delete, "Deleted customer", customer, null);
    }

    public async Task<IReadOnlyList<CustomerActivityLogDto>> GetCustomerActivitiesAsync(Guid id)
    {
        if (!await _customerRepository.ExistsAsync(id))
            throw new NotFoundException("Customer", id);

        var logs = await _customerRepository.GetActivityLogsAsync(id);
        return logs.Select(MapActivityLog).ToList();
    }

    private async Task EnsureActiveUserExistsAsync(Guid userId)
    {
        if (!await _context.Users.AnyAsync(u => u.Id == userId && u.IsActive))
            throw new ValidationException("assignedToId", "Người phụ trách không tồn tại hoặc đang bị tắt.");
    }

    private async Task TryLogAsync(Guid customerId, ActivityType action, string description, object? oldValues, object? newValues)
    {
        try
        {
            await _activityLogService.LogAsync("Customer", customerId, action, description, oldValues, newValues);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to write activity log for customer {CustomerId}", customerId);
        }
    }

    private static CustomerListItemDto MapListItem(Customer customer)
    {
        return new CustomerListItemDto
        {
            Id = customer.Id,
            FullName = customer.FullName,
            Phone = customer.Phone,
            Email = customer.Email,
            Company = customer.Company,
            Source = customer.Source,
            AssignedToId = customer.AssignedToId,
            AssignedToName = customer.AssignedTo?.FullName,
            ConvertedFromLeadId = customer.ConvertedFromLeadId,
            ConvertedFromLeadName = customer.ConvertedFromLead?.FullName,
            CreatedAt = customer.CreatedAt
        };
    }

    private static CustomerDetailDto MapDetail(Customer customer, IReadOnlyList<ActivityLog> logs)
    {
        return new CustomerDetailDto
        {
            Id = customer.Id,
            FullName = customer.FullName,
            Phone = customer.Phone,
            Email = customer.Email,
            Address = customer.Address,
            Company = customer.Company,
            Notes = customer.Notes,
            Source = customer.Source,
            AssignedToId = customer.AssignedToId,
            AssignedToName = customer.AssignedTo?.FullName,
            ConvertedFromLeadId = customer.ConvertedFromLeadId,
            ConvertedFromLeadName = customer.ConvertedFromLead?.FullName,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt,
            Activities = logs.Select(MapActivityLog).ToList()
        };
    }

    private static CustomerResponseDto MapResponse(Customer customer)
    {
        return new CustomerResponseDto
        {
            Id = customer.Id,
            FullName = customer.FullName,
            Phone = customer.Phone,
            Email = customer.Email,
            Address = customer.Address,
            Company = customer.Company,
            Notes = customer.Notes,
            Source = customer.Source,
            AssignedToId = customer.AssignedToId,
            ConvertedFromLeadId = customer.ConvertedFromLeadId,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
    }

    private static CustomerActivityLogDto MapActivityLog(ActivityLog log)
    {
        return new CustomerActivityLogDto
        {
            Id = log.Id,
            UserId = log.UserId,
            UserName = log.User?.FullName,
            EntityType = log.EntityType,
            EntityId = log.EntityId,
            Action = log.Action.ToString(),
            Description = log.Description,
            OldValues = log.OldValues,
            NewValues = log.NewValues,
            CreatedAt = log.CreatedAt
        };
    }

    private static string NormalizeRequired(string value, string field, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException(field, message);

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
