namespace RealSync.Core.Entities;

/// <summary>
/// AI Content Generation — lịch sử tạo nội dung bằng AI cho bài đăng.
/// </summary>
public class AIContentGeneration : BaseEntity
{
    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;

    public string Prompt { get; set; } = string.Empty;
    public string? GeneratedContent { get; set; }
}
