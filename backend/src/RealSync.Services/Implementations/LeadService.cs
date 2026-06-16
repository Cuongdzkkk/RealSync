using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RealSync.Core.Entities;
using RealSync.Core.Enums;
using RealSync.Core.Interfaces;
using RealSync.Data.Context;
using RealSync.Shared.DTOs.Requests.Leads;
using RealSync.Shared.DTOs.Responses.Leads;
using RealSync.Shared.Exceptions;
using ValidationException = RealSync.Shared.Exceptions.ValidationException;

namespace RealSync.Services.Implementations;

public class LeadService : ILeadService
{
    private static readonly Dictionary<string, string> StatusMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["New"] = "New",
        ["Contacted"] = "Contacted",
        ["Qualified"] = "Qualified",
        ["Proposal"] = "Proposal",
        ["Won"] = "Won",
        ["Lost"] = "Lost"
    };

    private static readonly Dictionary<string, string> PriorityMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Low"] = "Low",
        ["Normal"] = "Normal",
        ["High"] = "High",
        ["Urgent"] = "Urgent"
    };

    private readonly ILeadRepository _leadRepository;
    private readonly RealSyncDbContext _context;
    private readonly IActivityLogService _activityLogService;
    private readonly ILogger<LeadService> _logger;

    public LeadService(
        ILeadRepository leadRepository,
        RealSyncDbContext context,
        IActivityLogService activityLogService,
        ILogger<LeadService> logger)
    {
        _leadRepository = leadRepository;
        _context = context;
        _activityLogService = activityLogService;
        _logger = logger;
    }

    public async Task<(IReadOnlyList<LeadListItemDto> Items, int TotalCount)> GetLeadsAsync(LeadQueryDto query)
    {
        var (items, totalCount) = await _leadRepository.GetPagedAsync(query);
        return (items.Select(MapListItem).ToList(), totalCount);
    }

    public async Task<LeadDetailDto> GetLeadByIdAsync(Guid id)
    {
        var lead = await _leadRepository.GetDetailByIdAsync(id)
            ?? throw new NotFoundException("Lead", id);

        return MapDetail(lead);
    }

    public async Task<LeadResponseDto> CreateLeadAsync(LeadCreateDto dto)
    {
        var fullName = NormalizeRequired(dto.FullName, "fullName", "Tên lead không được để trống.");
        var phone = NormalizeOptional(dto.Phone);
        var email = NormalizeOptional(dto.Email);

        if (phone == null && email == null)
            throw new ValidationException("contact", "Vui lòng nhập số điện thoại hoặc email.");

        ValidateScore(dto.Score);
        ValidateBudget(dto.Budget);

        if (dto.InterestedPropertyId.HasValue)
            await EnsurePropertyExistsAsync(dto.InterestedPropertyId.Value);

        if (dto.AssignedToId.HasValue)
            await EnsureActiveUserExistsAsync(dto.AssignedToId.Value);

        var lead = new Lead
        {
            FullName = fullName,
            Phone = phone,
            Email = email,
            Status = NormalizeStatus(dto.Status),
            Priority = NormalizePriority(dto.Priority),
            Score = dto.Score ?? 0,
            InterestedPropertyId = dto.InterestedPropertyId,
            Budget = dto.Budget,
            Requirements = NormalizeOptional(dto.Requirements),
            PreferredArea = NormalizeOptional(dto.PreferredArea),
            PreferredType = NormalizeOptional(dto.PreferredType),
            AssignedToId = dto.AssignedToId,
            SourceChannel = NormalizeOptional(dto.SourceChannel),
            LastContactedAt = dto.LastContactedAt,
            NextFollowUpAt = dto.NextFollowUpAt
        };

        await _leadRepository.CreateAsync(lead);
        await TryLogAsync(lead.Id, ActivityType.Create, "Created lead", null, lead);

        return MapResponse(lead);
    }

    public async Task<LeadResponseDto> UpdateLeadAsync(Guid id, LeadUpdateDto dto)
    {
        var lead = await _leadRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Lead", id);

        if (dto.FullName != null)
            lead.FullName = NormalizeRequired(dto.FullName, "fullName", "Tên lead không được để trống.");

        if (dto.Phone != null)
            lead.Phone = NormalizeOptional(dto.Phone);

        if (dto.Email != null)
            lead.Email = NormalizeOptional(dto.Email);

        if (dto.Status != null)
            lead.Status = NormalizeStatus(dto.Status);

        if (dto.Priority != null)
            lead.Priority = NormalizePriority(dto.Priority);

        if (dto.Score.HasValue)
        {
            ValidateScore(dto.Score);
            lead.Score = dto.Score.Value;
        }

        if (dto.Budget.HasValue)
        {
            ValidateBudget(dto.Budget);
            lead.Budget = dto.Budget.Value;
        }

        if (dto.InterestedPropertyId.HasValue)
        {
            await EnsurePropertyExistsAsync(dto.InterestedPropertyId.Value);
            lead.InterestedPropertyId = dto.InterestedPropertyId.Value;
        }

        if (dto.AssignedToId.HasValue)
        {
            await EnsureActiveUserExistsAsync(dto.AssignedToId.Value);
            lead.AssignedToId = dto.AssignedToId.Value;
        }

        if (dto.Requirements != null)
            lead.Requirements = NormalizeOptional(dto.Requirements);

        if (dto.PreferredArea != null)
            lead.PreferredArea = NormalizeOptional(dto.PreferredArea);

        if (dto.PreferredType != null)
            lead.PreferredType = NormalizeOptional(dto.PreferredType);

        if (dto.SourceChannel != null)
            lead.SourceChannel = NormalizeOptional(dto.SourceChannel);

        if (dto.LastContactedAt.HasValue)
            lead.LastContactedAt = dto.LastContactedAt.Value;

        if (dto.NextFollowUpAt.HasValue)
            lead.NextFollowUpAt = dto.NextFollowUpAt.Value;

        if (string.IsNullOrWhiteSpace(lead.Phone) && string.IsNullOrWhiteSpace(lead.Email))
            throw new ValidationException("contact", "Lead phải có số điện thoại hoặc email.");

        await _leadRepository.UpdateAsync(lead);
        await TryLogAsync(lead.Id, ActivityType.Update, "Updated lead", null, lead);

        return MapResponse(lead);
    }

    public async Task DeleteLeadAsync(Guid id)
    {
        var lead = await _leadRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Lead", id);

        await _leadRepository.DeleteAsync(lead);
        await TryLogAsync(lead.Id, ActivityType.Delete, "Deleted lead", lead, null);
    }

    private async Task EnsurePropertyExistsAsync(Guid propertyId)
    {
        if (!await _context.Properties.AnyAsync(p => p.Id == propertyId))
            throw new ValidationException("interestedPropertyId", "Bất động sản quan tâm không tồn tại.");
    }

    private async Task EnsureActiveUserExistsAsync(Guid userId)
    {
        if (!await _context.Users.AnyAsync(u => u.Id == userId && u.IsActive))
            throw new ValidationException("assignedToId", "Người phụ trách không tồn tại hoặc đang bị tắt.");
    }

    private async Task TryLogAsync(Guid leadId, ActivityType action, string description, object? oldValues, object? newValues)
    {
        try
        {
            await _activityLogService.LogAsync("Lead", leadId, action, description, oldValues, newValues);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to write activity log for lead {LeadId}", leadId);
        }
    }

    private static LeadListItemDto MapListItem(Lead lead)
    {
        return new LeadListItemDto
        {
            Id = lead.Id,
            FullName = lead.FullName,
            Phone = lead.Phone,
            Email = lead.Email,
            Status = lead.Status,
            Priority = lead.Priority,
            Score = lead.Score,
            LeadTemperature = GetLeadTemperature(lead.Score),
            Budget = lead.Budget,
            PreferredArea = lead.PreferredArea,
            PreferredType = lead.PreferredType,
            SourceChannel = lead.SourceChannel,
            AssignedToId = lead.AssignedToId,
            AssignedToName = lead.AssignedTo?.FullName,
            InterestedPropertyId = lead.InterestedPropertyId,
            InterestedPropertyTitle = lead.InterestedProperty?.Title,
            LastContactedAt = lead.LastContactedAt,
            NextFollowUpAt = lead.NextFollowUpAt,
            CreatedAt = lead.CreatedAt
        };
    }

    private static LeadDetailDto MapDetail(Lead lead)
    {
        return new LeadDetailDto
        {
            Id = lead.Id,
            FullName = lead.FullName,
            Phone = lead.Phone,
            Email = lead.Email,
            Status = lead.Status,
            Priority = lead.Priority,
            Score = lead.Score,
            LeadTemperature = GetLeadTemperature(lead.Score),
            InterestedPropertyId = lead.InterestedPropertyId,
            InterestedPropertyTitle = lead.InterestedProperty?.Title,
            Budget = lead.Budget,
            Requirements = lead.Requirements,
            PreferredArea = lead.PreferredArea,
            PreferredType = lead.PreferredType,
            AssignedToId = lead.AssignedToId,
            AssignedToName = lead.AssignedTo?.FullName,
            SourceChannel = lead.SourceChannel,
            LastContactedAt = lead.LastContactedAt,
            NextFollowUpAt = lead.NextFollowUpAt,
            ConvertedAt = lead.ConvertedAt,
            CreatedAt = lead.CreatedAt,
            UpdatedAt = lead.UpdatedAt,
            Activities = lead.Activities
                .OrderByDescending(a => a.CreatedAt)
                .Select(MapActivity)
                .ToList()
        };
    }

    private static LeadResponseDto MapResponse(Lead lead)
    {
        return new LeadResponseDto
        {
            Id = lead.Id,
            FullName = lead.FullName,
            Phone = lead.Phone,
            Email = lead.Email,
            Status = lead.Status,
            Priority = lead.Priority,
            Score = lead.Score,
            LeadTemperature = GetLeadTemperature(lead.Score),
            InterestedPropertyId = lead.InterestedPropertyId,
            Budget = lead.Budget,
            Requirements = lead.Requirements,
            PreferredArea = lead.PreferredArea,
            PreferredType = lead.PreferredType,
            AssignedToId = lead.AssignedToId,
            SourceChannel = lead.SourceChannel,
            LastContactedAt = lead.LastContactedAt,
            NextFollowUpAt = lead.NextFollowUpAt,
            CreatedAt = lead.CreatedAt,
            UpdatedAt = lead.UpdatedAt
        };
    }

    private static LeadActivityDto MapActivity(LeadActivity activity)
    {
        return new LeadActivityDto
        {
            Id = activity.Id,
            ActivityType = activity.ActivityType,
            Description = activity.Description,
            OldValue = activity.OldValue,
            NewValue = activity.NewValue,
            PerformedById = activity.PerformedById,
            PerformedByName = activity.PerformedBy?.FullName,
            CreatedAt = activity.CreatedAt
        };
    }

    private static string GetLeadTemperature(int score)
    {
        return score >= 70 ? "Hot" : score >= 40 ? "Warm" : "Cold";
    }

    private static string NormalizeStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return "New";

        if (StatusMap.TryGetValue(status.Trim(), out var normalized))
            return normalized;

        throw new ValidationException("status", "Trạng thái lead không hợp lệ.");
    }

    private static string NormalizePriority(string? priority)
    {
        if (string.IsNullOrWhiteSpace(priority))
            return "Normal";

        if (PriorityMap.TryGetValue(priority.Trim(), out var normalized))
            return normalized;

        throw new ValidationException("priority", "Mức ưu tiên lead không hợp lệ.");
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

    private static void ValidateScore(int? score)
    {
        if (score is < 0 or > 100)
            throw new ValidationException("score", "Điểm lead phải từ 0 đến 100.");
    }

    private static void ValidateBudget(decimal? budget)
    {
        if (budget is < 0)
            throw new ValidationException("budget", "Ngân sách phải lớn hơn hoặc bằng 0.");
    }
}
