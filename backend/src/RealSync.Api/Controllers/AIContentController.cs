using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealSync.Api.Filters;
using RealSync.Core.Interfaces;
using RealSync.Shared.DTOs.Requests.Posts;
using RealSync.Shared.DTOs.Responses.Posts;

namespace RealSync.Api.Controllers;

/// <summary>
/// Controller tạo nội dung bài đăng bằng AI.
/// Route: /api/v1/posts/{postId}/ai-content
/// </summary>
[Authorize]
[Route("api/v1/posts/{postId:guid}/ai-content")]
[ApiController]
public class AIContentController : BaseController
{
    private readonly IAIContentService _aiContentService;

    public AIContentController(IAIContentService aiContentService)
    {
        _aiContentService = aiContentService;
    }

    /// <summary>
    /// Generate nội dung AI cho bài đăng.
    /// </summary>
    [HttpPost("generate")]
    [RequirePermission("posts.create")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<AIContentGenerationResponse>), 201)]
    public async Task<IActionResult> Generate(Guid postId, [FromBody] AIContentGenerateRequest request)
    {
        var result = await _aiContentService.GenerateAsync(postId, request);
        return CreatedResponse(result, "Nội dung AI đã được tạo");
    }

    /// <summary>
    /// Lịch sử generation của bài đăng.
    /// </summary>
    [HttpGet]
    [RequirePermission("posts.read")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<IEnumerable<AIContentGenerationResponse>>), 200)]
    public async Task<IActionResult> GetHistory(Guid postId)
    {
        var result = await _aiContentService.GetHistoryAsync(postId);
        return OkResponse(result);
    }

    /// <summary>
    /// Chi tiết một generation.
    /// </summary>
    [HttpGet("{id:guid}")]
    [RequirePermission("posts.read")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<AIContentGenerationResponse>), 200)]
    public async Task<IActionResult> GetById(Guid postId, Guid id)
    {
        var result = await _aiContentService.GetByIdAsync(postId, id);
        return OkResponse(result);
    }

    /// <summary>
    /// Áp dụng nội dung AI nháp vào bài đăng (Content Apply Flow).
    /// </summary>
    [HttpPost("apply")]
    [RequirePermission("posts.update")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<object>), 200)]
    public async Task<IActionResult> Apply(Guid postId, [FromBody] ApplyAIContentRequest request)
    {
        await _aiContentService.ApplyAsync(postId, request);
        return OkResponse<object>(null, "Nội dung bài đăng đã được cập nhật thành công.");
    }
}
