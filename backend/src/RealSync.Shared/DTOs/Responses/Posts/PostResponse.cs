namespace RealSync.Shared.DTOs.Responses.Posts;

/// <summary>
/// Response trả về thông tin bài đăng.
/// </summary>
public class PostResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? Summary { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? PublishedAt { get; set; }

    // Author info
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;

    // Property info
    public Guid? PropertyId { get; set; }
    public string? PropertyTitle { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Counts
    public int ChannelCount { get; set; }
    public int ScheduleCount { get; set; }
}
