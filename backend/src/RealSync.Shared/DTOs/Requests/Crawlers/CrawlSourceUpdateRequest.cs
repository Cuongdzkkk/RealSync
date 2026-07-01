namespace RealSync.Shared.DTOs.Requests.Crawlers;

public class CrawlSourceUpdateRequest
{
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int SuccessRate { get; set; }
}
