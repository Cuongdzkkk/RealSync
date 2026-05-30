namespace RealSync.Core.Entities;

/// <summary>
/// Quyền hạn trong hệ thống.
/// Ví dụ: "leads.create", "properties.read", "users.delete".
/// </summary>
public class Permission : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Group { get; set; }
    public string? Description { get; set; }

    // Navigation
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
