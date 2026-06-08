using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealSync.Api.Filters;
using RealSync.Core.Interfaces;
using RealSync.Shared.DTOs.Requests.Posts;
using RealSync.Shared.DTOs.Responses.Posts;

namespace RealSync.Api.Controllers;

/// <summary>
/// Controller quản lý kênh đăng bài.
/// Route: /api/v1/posts/{postId}/channels
/// </summary>
[Authorize]
[Route("api/v1/posts/{postId:guid}/channels")]
[ApiController]
public class PostChannelsController : BaseController
{
    private readonly IPostChannelService _channelService;

    public PostChannelsController(IPostChannelService channelService)
    {
        _channelService = channelService;
    }

    /// <summary>
    /// Lấy danh sách channels của post.
    /// </summary>
    [HttpGet]
    [RequirePermission("posts.read")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<IEnumerable<PostChannelResponse>>), 200)]
    public async Task<IActionResult> GetByPostId(Guid postId)
    {
        var result = await _channelService.GetByPostIdAsync(postId);
        return OkResponse(result);
    }

    /// <summary>
    /// Thêm channel cho post.
    /// </summary>
    [HttpPost]
    [RequirePermission("posts.publish")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<PostChannelResponse>), 201)]
    public async Task<IActionResult> Create(Guid postId, [FromBody] PostChannelCreateRequest request)
    {
        var result = await _channelService.CreateAsync(postId, request);
        return CreatedResponse(result, "Thêm kênh thành công");
    }

    /// <summary>
    /// Cập nhật channel.
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequirePermission("posts.publish")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<PostChannelResponse>), 200)]
    public async Task<IActionResult> Update(Guid postId, Guid id, [FromBody] PostChannelUpdateRequest request)
    {
        var result = await _channelService.UpdateAsync(postId, id, request);
        return OkResponse(result, "Cập nhật kênh thành công");
    }

    /// <summary>
    /// Xóa channel.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RequirePermission("posts.publish")]
    public async Task<IActionResult> Delete(Guid postId, Guid id)
    {
        await _channelService.DeleteAsync(postId, id);
        return NoContent();
    }

    /// <summary>
    /// Publish bài lên kênh.
    /// </summary>
    [HttpPost("{id:guid}/publish")]
    [RequirePermission("posts.publish")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<PostChannelResponse>), 200)]
    public async Task<IActionResult> Publish(Guid postId, Guid id)
    {
        var result = await _channelService.PublishAsync(postId, id);
        return OkResponse(result, "Publish thành công");
    }
}
