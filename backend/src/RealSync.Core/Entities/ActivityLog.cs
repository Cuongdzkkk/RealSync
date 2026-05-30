using RealSync.Core.Enums;

namespace RealSync.Core.Entities;

/// <summary>
/// Log hoạt động tổng quát của user trong hệ thống.
/// Ghi lại mọi thao tác CRUD, login, status change, etc.
/// </summary>
public class ActivityLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public string EntityType { get; set; } = string.Empty;  // "Property", "Lead", "User"
    public Guid? EntityId { get; set; }
    public ActivityType Action { get; set; }

    /// <summary>
    /// Mô tả ngắn gọn hành động.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// JSON chứa giá trị cũ (cho Update/Delete).
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// JSON chứa giá trị mới (cho Create/Update).
    /// </summary>
    public string? NewValues { get; set; }

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
