namespace RealSync.Core.Entities;

/// <summary>
/// Khu vực: Tỉnh/TP → Quận/Huyện → Phường/Xã.
/// Dùng self-referencing hierarchy (ParentId).
/// </summary>
public class Area : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public int Level { get; set; }  // 1=Tỉnh/TP, 2=Quận/Huyện, 3=Phường/Xã
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    // Hierarchy
    public Guid? ParentId { get; set; }
    public Area? Parent { get; set; }
    public ICollection<Area> Children { get; set; } = new List<Area>();

    // Navigation
    public ICollection<Property> Properties { get; set; } = new List<Property>();
}
