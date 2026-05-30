using RealSync.Core.Enums;

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
}
