namespace RealSync.Shared.DTOs.Responses.CrmAnalytics;

public class CrmCountByLabelDto
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}
