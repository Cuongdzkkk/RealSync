namespace RealSync.Core.Entities;

/// <summary>
/// Loại bất động sản: Đất nền, Nhà phố, Căn hộ, Biệt thự, ...
/// </summary>
public class PropertyType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Property> Properties { get; set; } = new List<Property>();
}
