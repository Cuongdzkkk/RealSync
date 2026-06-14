using RealSync.Shared.DTOs.Requests.Posts;
using RealSync.Shared.DTOs.Responses.Posts;

namespace RealSync.Core.Interfaces;

/// <summary>
/// Interface cho Post Analytics service — thống kê hiệu suất bài đăng.
/// </summary>
public interface IPostAnalyticsService
{
    Task<PostAnalyticsResponse> GetByPostIdAsync(Guid postId);
    Task<PostAnalyticsSummaryResponse> GetSummaryAsync();
    Task<PostAnalyticsResponse> UpdateAsync(Guid postId, PostAnalyticsUpdateRequest request);
}
