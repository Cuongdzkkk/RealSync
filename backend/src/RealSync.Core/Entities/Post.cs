namespace RealSync.Core.Entities;

/// <summary>
/// Bài đăng bất động sản.
/// Liên kết với Property (BĐS gắn bài) và User (tác giả).
/// </summary>
public class Post : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? Summary { get; set; }
    public string? ThumbnailUrl { get; set; }

    // Property link
    public Guid? PropertyId { get; set; }
    public Property? Property { get; set; }

    // Author
    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;

    // Status
    public string Status { get; set; } = "Draft";  // Draft, Scheduled, Published, Failed, Archived
    public DateTime? PublishedAt { get; set; }

    // Navigation
    public ICollection<PostChannel> PostChannels { get; set; } = new List<PostChannel>();
    public PostAnalytics? PostAnalytics { get; set; }
    public ICollection<PostSchedule> PostSchedules { get; set; } = new List<PostSchedule>();
    public ICollection<AIContentGeneration> AIContentGenerations { get; set; } = new List<AIContentGeneration>();
}
