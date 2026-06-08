using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealSync.Api.Filters;
using RealSync.Core.Interfaces;
using RealSync.Shared.DTOs.Requests.Posts;
using RealSync.Shared.DTOs.Responses.Posts;

namespace RealSync.Api.Controllers;

/// <summary>
/// Controller CRUD bài đăng BĐS.
/// </summary>
[Authorize]
public class PostsController : BaseController
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService)
    {
        _postService = postService;
    }

    /// <summary>
    /// Danh sách bài đăng (paginated, filter, search).
    /// </summary>
    [HttpGet]
    [RequirePermission("posts.read")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<IEnumerable<PostResponse>>), 200)]
    public async Task<IActionResult> GetList([FromQuery] PostFilterRequest filter)
    {
        var (items, totalCount) = await _postService.GetListAsync(filter);
        return PagedResponse(items, filter.Page, filter.PageSize, totalCount);
    }

    /// <summary>
    /// Chi tiết bài đăng.
    /// </summary>
    [HttpGet("{id:guid}")]
    [RequirePermission("posts.read")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<PostResponse>), 200)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _postService.GetByIdAsync(id);
        return OkResponse(result);
    }

    /// <summary>
    /// Tạo bài đăng mới.
    /// </summary>
    [HttpPost]
    [RequirePermission("posts.create")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<PostResponse>), 201)]
    public async Task<IActionResult> Create([FromBody] PostCreateRequest request)
    {
        var result = await _postService.CreateAsync(request);
        return CreatedResponse(result, "Tạo bài đăng thành công");
    }

    /// <summary>
    /// Cập nhật bài đăng.
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequirePermission("posts.update")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<PostResponse>), 200)]
    public async Task<IActionResult> Update(Guid id, [FromBody] PostUpdateRequest request)
    {
        var result = await _postService.UpdateAsync(id, request);
        return OkResponse(result, "Cập nhật bài đăng thành công");
    }

    /// <summary>
    /// Đổi trạng thái bài đăng.
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [RequirePermission("posts.update")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<PostResponse>), 200)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] PostStatusUpdateRequest request)
    {
        var result = await _postService.UpdateStatusAsync(id, request.Status);
        return OkResponse(result, "Cập nhật trạng thái thành công");
    }

    /// <summary>
    /// Xóa mềm bài đăng.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RequirePermission("posts.delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _postService.DeleteAsync(id);
        return NoContent();
    }
}
