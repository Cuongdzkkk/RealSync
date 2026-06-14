using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealSync.Api.Filters;
using RealSync.Core.Interfaces;
using RealSync.Shared.DTOs.Requests.Posts;
using RealSync.Shared.DTOs.Responses.Posts;

namespace RealSync.Api.Controllers;

/// <summary>
/// Controller thống kê hiệu suất bài đăng.
/// </summary>
[Authorize]
public class PostAnalyticsController : BaseController
{
    private readonly IPostAnalyticsService _analyticsService;

    public PostAnalyticsController(IPostAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    /// <summary>
    /// Analytics của một bài đăng.
    /// </summary>
    [HttpGet("/api/v1/posts/{postId:guid}/analytics")]
    [RequirePermission("posts.read")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<PostAnalyticsResponse>), 200)]
    public async Task<IActionResult> GetByPostId(Guid postId)
    {
        var result = await _analyticsService.GetByPostIdAsync(postId);
        return OkResponse(result);
    }

    /// <summary>
    /// Tổng hợp analytics tất cả bài đăng.
    /// </summary>
    [HttpGet("summary")]
    [RequirePermission("posts.read")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<PostAnalyticsSummaryResponse>), 200)]
    public async Task<IActionResult> GetSummary()
    {
        var result = await _analyticsService.GetSummaryAsync();
        return OkResponse(result);
    }

    /// <summary>
    /// Cập nhật analytics (system/internal).
    /// </summary>
    [HttpPut("/api/v1/posts/{postId:guid}/analytics")]
    [RequirePermission("posts.update")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<PostAnalyticsResponse>), 200)]
    public async Task<IActionResult> Update(Guid postId, [FromBody] PostAnalyticsUpdateRequest request)
    {
        var result = await _analyticsService.UpdateAsync(postId, request);
        return OkResponse(result, "Cập nhật analytics thành công");
    }
}
