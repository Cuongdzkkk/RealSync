namespace RealSync.Shared.DTOs.Responses.Posts;

/// <summary>
/// Response thông tin lịch đăng bài.
/// </summary>
public class PostScheduleResponse
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public string PostTitle { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
