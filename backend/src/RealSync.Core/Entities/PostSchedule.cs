namespace RealSync.Core.Entities;

/// <summary>
/// Lịch đăng bài — schedule publish cho Post.
/// </summary>
public class PostSchedule : BaseEntity
{
    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;

    public DateTime ScheduledAt { get; set; }
    public string Status { get; set; } = "Pending";  // Pending, Executing, Completed, Failed, Cancelled
}
