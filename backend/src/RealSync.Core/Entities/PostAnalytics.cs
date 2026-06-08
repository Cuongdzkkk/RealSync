namespace RealSync.Core.Entities;

/// <summary>
/// Thống kê bài đăng — 1-1 với Post.
/// </summary>
public class PostAnalytics : BaseEntity
{
    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;

    public int Views { get; set; } = 0;
    public int Clicks { get; set; } = 0;
    public int LeadsGenerated { get; set; } = 0;
    public decimal ConversionRate { get; set; } = 0;
    public DateTime? LastUpdated { get; set; }
}
