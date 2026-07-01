using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealSync.Core.Interfaces;
using RealSync.Shared.DTOs.Requests.Crawlers;
using RealSync.Shared.DTOs.Responses;
using RealSync.Shared.DTOs.Responses.Crawlers;

namespace RealSync.Api.Controllers;

/// <summary>
/// Controller quản lý tiến trình cào dữ liệu.
/// Route: /api/v1/crawlers
/// </summary>
[Authorize]
[Route("api/v1/crawlers")]
[ApiController]
public class CrawlerController : BaseController
{
    private readonly ICrawlerService _crawlerService;

    public CrawlerController(ICrawlerService crawlerService)
    {
        _crawlerService = crawlerService;
    }

    /// <summary>
    /// Lấy danh sách tất cả nguồn cào tin và số liệu thực từ DB.
    /// </summary>
    [HttpGet("sources")]
    [ProducesResponseType(typeof(ApiResponse<List<CrawlSourceDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSources()
    {
        var result = await _crawlerService.GetSourcesAsync();
        return OkResponse(result);
    }

    /// <summary>
    /// Tạo mới một nguồn cào.
    /// </summary>
    [HttpPost("sources")]
    [ProducesResponseType(typeof(ApiResponse<CrawlSourceDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateSource([FromBody] CrawlSourceCreateRequest request)
    {
        var result = await _crawlerService.CreateSourceAsync(request);
        return CreatedResponse(result, "Tạo nguồn cào thành công");
    }

    /// <summary>
    /// Cập nhật nguồn cào.
    /// </summary>
    [HttpPut("sources/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CrawlSourceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateSource(Guid id, [FromBody] CrawlSourceUpdateRequest request)
    {
        var result = await _crawlerService.UpdateSourceAsync(id, request);
        return OkResponse(result, "Cập nhật nguồn cào thành công");
    }

    /// <summary>
    /// Xóa mềm nguồn cào.
    /// </summary>
    [HttpDelete("sources/{id:guid}")]
    public async Task<IActionResult> DeleteSource(Guid id)
    {
        await _crawlerService.DeleteSourceAsync(id);
        return OkResponse(true, "Đã xóa nguồn cào");
    }

    /// <summary>
    /// Chạy cào tin mô phỏng và lưu property vào DB.
    /// </summary>
    [HttpPost("sources/{id:guid}/run")]
    public async Task<IActionResult> RunCrawler(Guid id, [FromBody] CrawlRunRequest request)
    {
        var result = await _crawlerService.RunCrawlerAsync(id, request);
        return OkResponse(result);
    }

    /// <summary>
    /// Lấy thống kê cào dữ liệu và phân loại AI từ DB.
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResponse<CrawlStatsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats()
    {
        var result = await _crawlerService.GetStatsAsync();
        return OkResponse(result);
    }

    /// <summary>
    /// Lấy lịch sử hoạt động cào tin gần đây.
    /// </summary>
    [HttpGet("jobs")]
    public async Task<IActionResult> GetJobs()
    {
        var result = await _crawlerService.GetJobsAsync();
        return OkResponse(result);
    }
}
