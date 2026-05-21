namespace RealSync.Core.Entities;

/// <summary>
/// Job crawl dữ liệu.
/// Schema theo database-guide.md Section 4.3.
/// </summary>
public class CrawlJob : BaseEntity
{
    public Guid CrawlSourceId { get; set; }
    public CrawlSource CrawlSource { get; set; } = null!;

    public string Status { get; set; } = "Pending";  // Pending, Running, Completed, Failed
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Stats
    public int TotalPages { get; set; } = 0;
    public int TotalItems { get; set; } = 0;
    public int SuccessCount { get; set; } = 0;
    public int ErrorCount { get; set; } = 0;
    public int DuplicateCount { get; set; } = 0;

    public string? ErrorMessage { get; set; }
    public string? ExecutionLog { get; set; }

    // Navigation
    public ICollection<CrawlResult> Results { get; set; } = new List<CrawlResult>();
    public ICollection<Property> Properties { get; set; } = new List<Property>();
}
