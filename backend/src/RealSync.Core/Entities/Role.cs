namespace RealSync.Core.Entities;

/// <summary>
/// Vai trò người dùng: Admin, Manager, Agent, Viewer.
/// </summary>
public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation
    public ICollection<User> Users { get; set; } = new List<User>();
}
