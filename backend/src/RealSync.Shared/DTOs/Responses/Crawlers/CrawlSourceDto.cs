namespace RealSync.Shared.DTOs.Responses.Crawlers;

public class CrawlSourceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int SuccessRate { get; set; }
    public int ListingsToday { get; set; }
    public DateTime LastRunAt { get; set; }
}
