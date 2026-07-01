namespace RealSync.Shared.DTOs.Requests.Crawlers;

public class CrawlSourceCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SuccessRate { get; set; } = 95;
}
