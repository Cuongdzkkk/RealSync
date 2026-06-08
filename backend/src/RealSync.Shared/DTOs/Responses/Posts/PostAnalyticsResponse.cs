namespace RealSync.Shared.DTOs.Responses.Posts;

/// <summary>
/// Response thống kê hiệu suất bài đăng.
/// </summary>
public class PostAnalyticsResponse
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public string PostTitle { get; set; } = string.Empty;
    public int Views { get; set; }
    public int Clicks { get; set; }
    public int LeadsGenerated { get; set; }
    public decimal ConversionRate { get; set; }
    public DateTime? LastUpdated { get; set; }
}
