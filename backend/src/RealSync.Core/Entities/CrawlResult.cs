namespace RealSync.Core.Entities;

/// <summary>
/// Kết quả crawl chi tiết cho từng item.
/// </summary>
public class CrawlResult : BaseEntity
{
    public Guid CrawlJobId { get; set; }
    public CrawlJob CrawlJob { get; set; } = null!;

    public string Url { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";  // Pending, Success, Failed, Duplicate
    public string? RawData { get; set; }  // JSON raw data
    public string? ErrorMessage { get; set; }
    public string? ContentHash { get; set; }  // Để check trùng lặp

    // Linked property (nếu đã parse thành công)
    public Guid? PropertyId { get; set; }
    public Property? Property { get; set; }
}
