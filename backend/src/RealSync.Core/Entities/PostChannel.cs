namespace RealSync.Core.Entities;

/// <summary>
/// Kênh đăng bài — theo dõi trạng thái publish trên từng kênh.
/// </summary>
public class PostChannel : BaseEntity
{
    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;

    public string Channel { get; set; } = string.Empty;  // Website, Facebook, Batdongsan, Chotot, Zalo
    public string PublishStatus { get; set; } = "Pending";  // Pending, Publishing, Published, Failed
    public string? PublishedUrl { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? ErrorMessage { get; set; }
}
