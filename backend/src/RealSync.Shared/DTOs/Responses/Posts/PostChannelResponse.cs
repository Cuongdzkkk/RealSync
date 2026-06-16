namespace RealSync.Shared.DTOs.Responses.Posts;

/// <summary>
/// Response thông tin kênh đăng bài.
/// </summary>
public class PostChannelResponse
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string PublishStatus { get; set; } = string.Empty;
    public string? PublishedUrl { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}
