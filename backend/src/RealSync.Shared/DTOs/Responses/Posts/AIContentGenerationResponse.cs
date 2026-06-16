namespace RealSync.Shared.DTOs.Responses.Posts;

/// <summary>
/// Response thông tin AI content generation.
/// </summary>
public class AIContentGenerationResponse
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public string? GeneratedContent { get; set; }
    public DateTime CreatedAt { get; set; }
}
