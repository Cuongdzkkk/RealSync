namespace RealSync.Core.Entities;

/// <summary>
/// Nguồn crawl (website BĐS).
/// </summary>
public class CrawlSource : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public string? CrawlConfig { get; set; }  // JSON config cho crawl rules
    public string? CronSchedule { get; set; }  // Cron expression cho scheduling

    // Navigation
    public ICollection<CrawlJob> CrawlJobs { get; set; } = new List<CrawlJob>();
}
