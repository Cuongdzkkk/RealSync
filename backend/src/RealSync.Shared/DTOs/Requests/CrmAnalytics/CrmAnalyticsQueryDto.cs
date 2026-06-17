namespace RealSync.Shared.DTOs.Requests.CrmAnalytics;

public class CrmAnalyticsQueryDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public Guid? AssignedToId { get; set; }
    public string? SourceChannel { get; set; }
    public string? Status { get; set; }
}
