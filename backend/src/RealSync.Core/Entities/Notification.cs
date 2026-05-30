using RealSync.Core.Enums;

namespace RealSync.Core.Entities;

/// <summary>
/// Thông báo hệ thống cho người dùng.
/// </summary>
public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.System;

    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// JSON data bổ sung (entityId, entityType, etc.)
    /// </summary>
    public string? Data { get; set; }

    /// <summary>
    /// Link điều hướng khi click thông báo.
    /// </summary>
    public string? Link { get; set; }
}
