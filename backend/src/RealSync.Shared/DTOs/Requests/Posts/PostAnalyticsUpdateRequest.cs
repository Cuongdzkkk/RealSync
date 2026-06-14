namespace RealSync.Shared.DTOs.Requests.Posts;

/// <summary>
/// Request cập nhật analytics cho bài đăng.
/// </summary>
public class PostAnalyticsUpdateRequest
{
    public int Views { get; set; }
    public int Clicks { get; set; }
    public int LeadsGenerated { get; set; }
}
