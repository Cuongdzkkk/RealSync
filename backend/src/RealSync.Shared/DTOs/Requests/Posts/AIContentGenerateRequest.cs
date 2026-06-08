namespace RealSync.Shared.DTOs.Requests.Posts;

/// <summary>
/// Request tạo nội dung bằng AI.
/// </summary>
public class AIContentGenerateRequest
{
    public string Prompt { get; set; } = string.Empty;
    public Guid? PropertyId { get; set; }
}
