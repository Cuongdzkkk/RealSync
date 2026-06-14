namespace RealSync.Shared.DTOs.Responses.Properties;

public class PropertyImageDto
{
    public Guid Id { get; set; }
    public Guid PropertyId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public bool IsThumbnail { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}
