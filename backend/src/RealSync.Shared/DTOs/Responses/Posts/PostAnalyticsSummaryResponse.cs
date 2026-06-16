namespace RealSync.Shared.DTOs.Responses.Posts;

/// <summary>
/// Response tổng hợp analytics tất cả bài đăng.
/// </summary>
public class PostAnalyticsSummaryResponse
{
    public int TotalPosts { get; set; }
    public int TotalPublished { get; set; }
    public int TotalViews { get; set; }
    public int TotalClicks { get; set; }
    public int TotalLeadsGenerated { get; set; }
    public decimal AverageConversionRate { get; set; }
}
