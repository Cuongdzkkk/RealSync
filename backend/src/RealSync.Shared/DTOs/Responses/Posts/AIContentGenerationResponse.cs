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

    // Usage Tracking
    public int? PromptTokens { get; set; }
    public int? CompletionTokens { get; set; }
    public decimal? EstimatedCost { get; set; }
    public string? FactsUsedJson { get; set; }
}
