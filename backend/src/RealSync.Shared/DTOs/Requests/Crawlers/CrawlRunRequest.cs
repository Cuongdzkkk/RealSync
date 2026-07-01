namespace RealSync.Shared.DTOs.Requests.Crawlers;

public class CrawlRunRequest
{
    public string Area { get; set; } = "Quận 7";
    public string Province { get; set; } = "TP. Hồ Chí Minh";
    public string PropertyType { get; set; } = "Căn hộ";
    public string Category { get; set; } = "Nhà đất bán";
    public bool EnableAiFilter { get; set; } = true;
    public string CrawlMode { get; set; } = "Property"; // "Property" or "Lead"
    public string? Prompt { get; set; } // Prompt search query for AI Lead matching
    public bool UseLocationFilter { get; set; } = true; // Whether to filter by area/province
}
