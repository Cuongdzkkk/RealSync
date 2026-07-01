namespace RealSync.Shared.DTOs.Requests.Posts;

public record ApplyAIContentRequest(
    string Content,
    string? Summary = null
);
