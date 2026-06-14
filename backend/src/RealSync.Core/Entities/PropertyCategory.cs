namespace RealSync.Core.Entities;

/// <summary>
/// Nhóm danh mục bất động sản dùng cho phân loại nội dung/listing.
/// </summary>
public class PropertyCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Property> Properties { get; set; } = new List<Property>();
}
