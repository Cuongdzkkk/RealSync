using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealSync.Api.Filters;
using RealSync.Core.Entities;
using RealSync.Core.Interfaces.Publishing;
using RealSync.Shared.DTOs.Responses;

using RealSync.Shared.Exceptions;

namespace RealSync.Api.Controllers;

/// <summary>
/// Controller quản lý tiến trình sinh video AI (Veo & FFmpeg).
/// </summary>
[Authorize]
public class VideoProjectsController : BaseController
{
    private readonly IVideoProjectService _videoProjectService;

    public VideoProjectsController(IVideoProjectService videoProjectService)
    {
        _videoProjectService = videoProjectService;
    }

    [HttpPost]
    [RequirePermission("posts.update")]
    [ProducesResponseType(typeof(ApiResponse<VideoProject>), 201)]
    public async Task<IActionResult> Create([FromBody] CreateVideoProjectRequest request, CancellationToken cancellationToken)
    {
        var result = await _videoProjectService.CreateProjectAsync(request.ContentVariantId, request.PostId, cancellationToken);
        return CreatedResponse(result, "Tạo dự án dựng video AI thành công.");
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("posts.read")]
    [ProducesResponseType(typeof(ApiResponse<VideoProject>), 200)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _videoProjectService.GetProjectByIdAsync(id, cancellationToken);
        if (result == null)
        {
            throw new NotFoundException("VideoProject", id);
        }
        return OkResponse(result);
    }

    [HttpPut("{id:guid}/storyboard")]
    [RequirePermission("posts.update")]
    [ProducesResponseType(typeof(ApiResponse<VideoProject>), 200)]
    public async Task<IActionResult> UpdateStoryboard(Guid id, [FromBody] List<VideoSceneUpdateDto> scenes, CancellationToken cancellationToken)
    {
        var result = await _videoProjectService.UpdateStoryboardAsync(id, scenes, cancellationToken);
        return OkResponse(result, "Cập nhật kịch bản phân cảnh thành công.");
    }

    [HttpPost("{id:guid}/generate")]
    [RequirePermission("posts.update")]
    [ProducesResponseType(typeof(ApiResponse<VideoGenerationJob>), 200)]
    public async Task<IActionResult> GenerateVideo(Guid id, CancellationToken cancellationToken)
    {
        var result = await _videoProjectService.StartGenerationAsync(id, cancellationToken);
        return OkResponse(result, "Đã khởi tạo yêu cầu sinh các phân cảnh video.");
    }

    [HttpPost("{id:guid}/render")]
    [RequirePermission("posts.update")]
    [ProducesResponseType(typeof(ApiResponse<VideoGenerationJob>), 200)]
    public async Task<IActionResult> RenderVideo(Guid id, CancellationToken cancellationToken)
    {
        var result = await _videoProjectService.StartRenderingAsync(id, cancellationToken);
        return OkResponse(result, "Đã khởi tạo yêu cầu ghép nối và xuất video thành phẩm.");
    }
}

public class CreateVideoProjectRequest
{
    public Guid ContentVariantId { get; set; }
    public Guid? PostId { get; set; }
}
