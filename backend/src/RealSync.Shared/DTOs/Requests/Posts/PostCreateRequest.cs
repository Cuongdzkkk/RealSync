using RealSync.Shared.DTOs.Requests;

namespace RealSync.Shared.DTOs.Requests.Posts;

/// <summary>
/// Request tạo bài đăng mới.
/// </summary>
public class PostCreateRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? Summary { get; set; }
    public string? ThumbnailUrl { get; set; }
    public Guid? PropertyId { get; set; }
}
