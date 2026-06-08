namespace RealSync.Core.Entities;

/// <summary>
/// Hình ảnh bất động sản.
/// </summary>
public class PropertyImage : BaseEntity
{
    public Guid PropertyId { get; set; }
    public Property Property { get; set; } = null!;

    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string? Caption { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsThumbnail { get; set; } = false;
    public bool IsPrimary { get; set; } = false;
}
