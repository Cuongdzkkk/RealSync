namespace RealSync.Core.Entities;

/// <summary>
/// Hình ảnh bất động sản.
/// </summary>
public class PropertyImage : BaseEntity
{
    public Guid PropertyId { get; set; }
    public Property Property { get; set; } = null!;

    public string Url { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsPrimary { get; set; } = false;
}
