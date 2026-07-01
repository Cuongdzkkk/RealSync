namespace RealSync.Shared.DTOs.Responses.Crawlers;

public class CrawlStatsDto
{
    public int TotalClassified { get; set; }
    public int AvgLatencyMs { get; set; }
    public double Accuracy { get; set; }
    public double AcceptanceRate { get; set; }
}
