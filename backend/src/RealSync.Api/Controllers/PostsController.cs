using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealSync.Api.Filters;
using RealSync.Core.Interfaces;
using RealSync.Data.Context;
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
    private readonly RealSyncDbContext _context;

    public PostsController(IPostService postService, RealSyncDbContext context)
    {
        _postService = postService;
        _context = context;
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

    /// <summary>
    /// Trang showcase công khai hiển thị chi tiết bất động sản.
    /// </summary>
    [HttpGet("{id:guid}/public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicShowcase(Guid id)
    {
        var post = await _context.Posts
            .Include(p => p.Property)
            .FirstOrDefaultAsync(p => p.Id == id);
        
        if (post == null)
            return NotFound("Không tìm thấy bài viết.");

        var html = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <title>{post.Title}</title>
            <meta charset='utf-8' />
            <meta name='viewport' content='width=device-width, initial-scale=1.0' />
            <meta property='og:title' content='{post.Title}' />
            <meta property='og:description' content='{post.Summary}' />
            <link rel='stylesheet' href='https://fonts.googleapis.com/css2?family=Outfit:wght@400;600;700&display=swap'>
            <style>
                body {{ font-family: 'Outfit', sans-serif; background: #0b0f19; color: #f3f4f6; margin: 0; padding: 40px; }}
                .card {{ max-width: 800px; margin: auto; background: rgba(255,255,255,0.03); border: 1px solid rgba(255,255,255,0.08); border-radius: 16px; padding: 40px; box-shadow: 0 8px 32px rgba(0,0,0,0.5); backdrop-filter: blur(12px); }}
                h1 {{ color: #fbbf24; font-size: 32px; margin-top: 0; }}
                p {{ line-height: 1.8; font-size: 16px; color: #d1d5db; }}
                .badge {{ display: inline-block; background: #3b82f6; color: white; padding: 6px 12px; border-radius: 99px; font-size: 14px; font-weight: 600; margin-bottom: 20px; }}
                .price {{ font-size: 24px; font-weight: bold; color: #10b981; margin: 20px 0; }}
                .cta-btn {{ display: block; text-align: center; background: linear-gradient(135deg, #fbbf24, #f59e0b); color: #0b0f19; font-weight: bold; text-decoration: none; padding: 15px; border-radius: 8px; margin-top: 30px; box-shadow: 0 4px 14px rgba(245, 158, 11, 0.4); }}
            </style>
        </head>
        <body>
            <div class='card'>
                <span class='badge'>BĐS RealSync</span>
                <h1>{post.Title}</h1>
                <div class='price'>Giá: {(post.Property?.Price != null ? (post.Property.Price / 1000000000m).ToString("0.0") + " tỷ" : "Thỏa thuận")}</div>
                <p>{post.Content}</p>
                <a href='#' class='cta-btn'>Liên hệ Nhận Tư vấn Ngay</a>
            </div>
        </body>
        </html>";

        return Content(html, "text/html", System.Text.Encoding.UTF8);
    }
}
