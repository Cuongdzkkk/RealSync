using RealSync.Shared.DTOs.Requests.Posts;
using RealSync.Shared.DTOs.Responses.Posts;

namespace RealSync.Core.Interfaces;

/// <summary>
/// Interface cho AI Content Generation service — tạo nội dung bài đăng bằng AI.
/// </summary>
public interface IAIContentService
{
    Task<AIContentGenerationResponse> GenerateAsync(Guid postId, AIContentGenerateRequest request);
    Task<IEnumerable<AIContentGenerationResponse>> GetHistoryAsync(Guid postId);
    Task<AIContentGenerationResponse> GetByIdAsync(Guid postId, Guid id);
    Task<string> ChatAsync(string message);
}
