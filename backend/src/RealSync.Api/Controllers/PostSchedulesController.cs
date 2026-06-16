using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealSync.Api.Filters;
using RealSync.Core.Interfaces;
using RealSync.Shared.DTOs.Requests.Posts;
using RealSync.Shared.DTOs.Responses.Posts;

namespace RealSync.Api.Controllers;

/// <summary>
/// Controller lên lịch đăng bài.
/// </summary>
[Authorize]
public class PostSchedulesController : BaseController
{
    private readonly IPostScheduleService _scheduleService;

    public PostSchedulesController(IPostScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    /// <summary>
    /// Lấy lịch của một bài đăng.
    /// </summary>
    [HttpGet("/api/v1/posts/{postId:guid}/schedules")]
    [RequirePermission("posts.read")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<IEnumerable<PostScheduleResponse>>), 200)]
    public async Task<IActionResult> GetByPostId(Guid postId)
    {
        var result = await _scheduleService.GetByPostIdAsync(postId);
        return OkResponse(result);
    }

    /// <summary>
    /// Lên lịch đăng bài.
    /// </summary>
    [HttpPost("/api/v1/posts/{postId:guid}/schedules")]
    [RequirePermission("posts.publish")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<PostScheduleResponse>), 201)]
    public async Task<IActionResult> Create(Guid postId, [FromBody] ScheduleCreateRequest request)
    {
        var result = await _scheduleService.CreateAsync(postId, request);
        return CreatedResponse(result, "Lên lịch thành công");
    }

    /// <summary>
    /// Hủy lịch đăng bài.
    /// </summary>
    [HttpDelete("/api/v1/posts/{postId:guid}/schedules/{id:guid}")]
    [RequirePermission("posts.publish")]
    public async Task<IActionResult> Delete(Guid postId, Guid id)
    {
        await _scheduleService.DeleteAsync(postId, id);
        return NoContent();
    }

    /// <summary>
    /// Danh sách lịch sắp tới (tất cả posts).
    /// </summary>
    [HttpGet("upcoming")]
    [RequirePermission("posts.read")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<IEnumerable<PostScheduleResponse>>), 200)]
    public async Task<IActionResult> GetUpcoming([FromQuery] int count = 20)
    {
        var result = await _scheduleService.GetUpcomingAsync(count);
        return OkResponse(result);
    }
}
