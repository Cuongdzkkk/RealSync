using RealSync.Core.Enums;
using RealSync.Shared.DTOs.Requests.ActivityLogs;
using RealSync.Shared.DTOs.Responses.ActivityLogs;

namespace RealSync.Core.Interfaces;

/// <summary>
/// Service ghi log hoạt động của user.
/// </summary>
public interface IActivityLogService
{
    Task LogAsync(
        string entityType,
        Guid? entityId,
        ActivityType action,
        string? description = null,
        object? oldValues = null,
        object? newValues = null);

    Task LogForUserAsync(
        Guid? userId,
        string entityType,
        Guid? entityId,
        ActivityType action,
        string? description = null,
        object? oldValues = null,
        object? newValues = null);

    Task<(IReadOnlyList<ActivityLogDto> Items, int TotalCount)> GetActivityLogsAsync(ActivityLogQueryDto query);
}
