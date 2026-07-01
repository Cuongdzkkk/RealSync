using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealSync.Api.Filters;
using RealSync.Core.Enums;
using RealSync.Core.Interfaces;
using RealSync.Core.Interfaces.Publishing;
using RealSync.Shared.DTOs.Requests.Publishing;
using RealSync.Shared.DTOs.Responses;
using RealSync.Shared.DTOs.Responses.Publishing;

namespace RealSync.Api.Controllers;

/// <summary>
/// Controller quản lý quá trình xuất bản (Publishing Engine).
/// </summary>
[Authorize]
public class PublicationsController : BaseController
{
    private readonly IPublicationOrchestrator _orchestrator;
    private readonly IPublicationService _publicationService;

    public PublicationsController(IPublicationOrchestrator orchestrator, IPublicationService publicationService)
    {
        _orchestrator = orchestrator;
        _publicationService = publicationService;
    }

    /// <summary>
    /// Lấy danh sách các jobs xuất bản (phân trang, lọc theo post, status).
    /// </summary>
    [HttpGet("jobs")]
    [RequirePermission("posts.read")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PublicationJobResponse>>), 200)]
    public async Task<IActionResult> GetJobs(
        [FromQuery] Guid? postId,
        [FromQuery] PublicationJobStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _publicationService.GetJobsAsync(postId, status, page, pageSize, cancellationToken);
        return PagedResponse(items, page, pageSize, totalCount);
    }

    /// <summary>
    /// Đưa một bài viết vào hàng đợi xuất bản.
    /// </summary>
    [HttpPost("jobs")]
    [RequirePermission("posts.update")]
    [ProducesResponseType(typeof(ApiResponse<PublicationJobResponse>), 201)]
    public async Task<IActionResult> QueueJob(
        [FromBody] QueuePublicationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _orchestrator.QueueAsync(request, cancellationToken);
        return CreatedResponse(result, "Đã đưa bài viết vào hàng đợi xuất bản thành công.");
    }

    /// <summary>
    /// Thực hiện thử lại một job xuất bản bị lỗi.
    /// </summary>
    [HttpPost("jobs/{id:guid}/retry")]
    [RequirePermission("posts.update")]
    [ProducesResponseType(typeof(ApiResponse<PublicationJobResponse>), 200)]
    public async Task<IActionResult> RetryJob(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _orchestrator.RetryAsync(id, cancellationToken);
        return OkResponse(result, "Đã kích hoạt thử lại job xuất bản.");
    }

    /// <summary>
    /// Hủy một job xuất bản chưa hoàn thành.
    /// </summary>
    [HttpPost("jobs/{id:guid}/cancel")]
    [RequirePermission("posts.update")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> CancelJob(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _orchestrator.CancelAsync(id, cancellationToken);
        return OkResponse<object>(null!, "Đã hủy job xuất bản thành công.");
    }

    /// <summary>
    /// Làm mới trạng thái remote từ provider (TikTok processing, v.v.).
    /// </summary>
    [HttpPost("jobs/{id:guid}/refresh-status")]
    [RequirePermission("posts.update")]
    [ProducesResponseType(typeof(ApiResponse<PublicationJobResponse>), 200)]
    public async Task<IActionResult> RefreshJobStatus(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _orchestrator.RefreshRemoteStatusAsync(id, cancellationToken);
        var response = await _publicationService.GetJobByIdAsync(id, cancellationToken);
        return OkResponse(response, "Đã làm mới trạng thái từ provider.");
    }

    /// <summary>
    /// Lấy lịch sử các lượt thử (attempts) của một job xuất bản cụ thể.
    /// </summary>
    [HttpGet("jobs/{id:guid}/attempts")]
    [RequirePermission("posts.read")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PublicationAttemptResponse>>), 200)]
    public async Task<IActionResult> GetAttempts(
        Guid id,
        CancellationToken cancellationToken)
    {
        var attempts = await _publicationService.GetAttemptsAsync(id, cancellationToken);
        return OkResponse(attempts);
    }
}
