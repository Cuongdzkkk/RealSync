namespace RealSync.Core.Entities;

/// <summary>
/// Bảng liên kết nhiều-nhiều giữa Role và Permission.
/// Không kế thừa BaseEntity vì là join table.
/// </summary>
public class RolePermission
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
}
