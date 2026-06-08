namespace RealSync.Shared.DTOs.Requests.Posts;

/// <summary>
/// Request cập nhật bài đăng.
/// </summary>
public class PostUpdateRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? Summary { get; set; }
    public string? ThumbnailUrl { get; set; }
    public Guid? PropertyId { get; set; }
}
