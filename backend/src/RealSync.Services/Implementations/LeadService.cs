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
    private readonly INotificationService _notificationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<LeadService> _logger;

    public LeadService(
        ILeadRepository leadRepository,
        RealSyncDbContext context,
        IActivityLogService activityLogService,
        INotificationService notificationService,
        ICurrentUserService currentUserService,
        ILogger<LeadService> logger)
    {
        _leadRepository = leadRepository;
        _context = context;
        _activityLogService = activityLogService;
        _notificationService = notificationService;
        _currentUserService = currentUserService;
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

    public async Task<LeadResponseDto> UpdateStatusAsync(Guid id, LeadStatusUpdateDto dto)
    {
        var lead = await _leadRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Lead", id);

        var oldStatus = lead.Status;
        var newStatus = NormalizeStatus(dto.Status);
        var note = NormalizeOptional(dto.Note);

        if (string.Equals(oldStatus, newStatus, StringComparison.Ordinal) && note == null)
            return MapResponse(lead);

        lead.Status = newStatus;
        if (newStatus == "Won" && lead.ConvertedAt == null)
            lead.ConvertedAt = DateTime.UtcNow;

        await _leadRepository.UpdateAsync(lead);
        await AddSystemActivityAsync(
            lead.Id,
            "StatusChange",
            note ?? $"Changed status from {oldStatus} to {newStatus}",
            oldStatus,
            newStatus);
        await TryLogAsync(lead.Id, ActivityType.Update, "Updated lead status", new { Status = oldStatus }, new { Status = newStatus });

        return MapResponse(lead);
    }

    public async Task<LeadResponseDto> AssignLeadAsync(Guid id, LeadAssignDto dto)
    {
        var lead = await _leadRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Lead", id);

        await EnsureActiveUserExistsAsync(dto.AssignedToId);

        var oldAssignedToId = lead.AssignedToId;
        lead.AssignedToId = dto.AssignedToId;

        await _leadRepository.UpdateAsync(lead);
        await AddSystemActivityAsync(
            lead.Id,
            "Assigned",
            NormalizeOptional(dto.Note) ?? "Assigned lead to user",
            oldAssignedToId?.ToString(),
            dto.AssignedToId.ToString());
        await TrySendNotificationAsync(
            dto.AssignedToId,
            "Bạn được giao lead mới",
            $"Lead {lead.FullName} đã được giao cho bạn xử lý.",
            NotificationType.Assignment,
            $"/leads/{lead.Id}",
            new { leadId = lead.Id, fullName = lead.FullName, assignedToId = dto.AssignedToId });
        await TryLogAsync(lead.Id, ActivityType.Assignment, "Assigned lead", new { AssignedToId = oldAssignedToId }, new { dto.AssignedToId });

        return MapResponse(lead);
    }

    public async Task<LeadActivityDto> AddActivityAsync(Guid id, LeadActivityCreateDto dto)
    {
        var lead = await _leadRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Lead", id);

        var activityType = NormalizeClientActivityType(dto.ActivityType);
        var description = NormalizeOptional(dto.Description);

        if (activityType == "Note" && description == null)
            throw new ValidationException("description", "Ghi chú nội bộ cần có nội dung.");

        if (activityType is "Call" or "Email" or "Meeting")
        {
            lead.LastContactedAt = DateTime.UtcNow;
            await _leadRepository.UpdateAsync(lead);
        }

        var activity = await _leadRepository.AddActivityAsync(new LeadActivity
        {
            LeadId = id,
            ActivityType = activityType,
            Description = description,
            PerformedById = _currentUserService.UserId
        });

        await TryLogAsync(id, ActivityType.Update, $"Added lead activity: {activityType}", null, activity);
        return MapActivity(activity);
    }

    public async Task<IReadOnlyList<LeadActivityDto>> GetActivitiesAsync(Guid id)
    {
        if (!await _leadRepository.HasLeadAsync(id))
            throw new NotFoundException("Lead", id);

        var activities = await _leadRepository.GetActivitiesAsync(id);
        return activities.Select(MapActivity).ToList();
    }

    public async Task<LeadResponseDto> SetFollowUpAsync(Guid id, LeadFollowUpDto dto)
    {
        var lead = await _leadRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Lead", id);

        if (dto.NextFollowUpAt <= DateTime.UtcNow)
            throw new ValidationException("nextFollowUpAt", "Thời gian chăm sóc tiếp theo phải ở tương lai.");

        var oldFollowUpAt = lead.NextFollowUpAt;
        lead.NextFollowUpAt = dto.NextFollowUpAt;

        await _leadRepository.UpdateAsync(lead);
        await AddSystemActivityAsync(
            lead.Id,
            "FollowUp",
            NormalizeOptional(dto.Note) ?? $"Set next follow-up at {dto.NextFollowUpAt:O}",
            oldFollowUpAt?.ToString("O"),
            dto.NextFollowUpAt.ToString("O"));

        if (lead.AssignedToId.HasValue)
        {
            await TrySendNotificationAsync(
                lead.AssignedToId.Value,
                "Lịch chăm sóc lead",
                $"Lead {lead.FullName} có lịch chăm sóc vào {dto.NextFollowUpAt:O}.",
                NotificationType.Lead,
                $"/leads/{lead.Id}",
                new { leadId = lead.Id, nextFollowUpAt = dto.NextFollowUpAt });
        }

        await TryLogAsync(lead.Id, ActivityType.Update, "Updated lead follow-up", new { NextFollowUpAt = oldFollowUpAt }, new { dto.NextFollowUpAt });
        return MapResponse(lead);
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

    private async Task<LeadActivity> AddSystemActivityAsync(
        Guid leadId,
        string activityType,
        string description,
        string? oldValue,
        string? newValue)
    {
        return await _leadRepository.AddActivityAsync(new LeadActivity
        {
            LeadId = leadId,
            ActivityType = activityType,
            Description = description,
            OldValue = oldValue,
            NewValue = newValue,
            PerformedById = _currentUserService.UserId
        });
    }

    private async Task TrySendNotificationAsync(
        Guid userId,
        string title,
        string message,
        NotificationType type,
        string? link,
        object? data)
    {
        try
        {
            await _notificationService.SendAsync(userId, title, message, type, link, data);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send lead notification to user {UserId}", userId);
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

    private static string NormalizeClientActivityType(string? activityType)
    {
        if (string.IsNullOrWhiteSpace(activityType))
            throw new ValidationException("activityType", "Loại hoạt động không được để trống.");

        var trimmed = activityType.Trim();
        return trimmed.ToLowerInvariant() switch
        {
            "call" => "Call",
            "email" => "Email",
            "meeting" => "Meeting",
            "note" => "Note",
            _ => throw new ValidationException("activityType", "Loại hoạt động lead không hợp lệ.")
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
