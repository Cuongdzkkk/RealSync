using Microsoft.AspNetCore.Mvc;
using RealSync.Shared.DTOs.Responses;

namespace RealSync.Api.Controllers;

/// <summary>
/// Base controller cho toàn bộ API.
/// Enforce versioned route prefix và cung cấp helper methods cho response.
/// Tất cả controllers mới PHẢI kế thừa từ class này.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Trả 200 OK với data.
    /// </summary>
    protected IActionResult OkResponse<T>(T data, string message = "Thành công")
    {
        return Ok(ApiResponse<T>.Ok(data, message));
    }

    /// <summary>
    /// Trả 201 Created với data.
    /// </summary>
    protected IActionResult CreatedResponse<T>(T data, string message = "Tạo thành công")
    {
        return StatusCode(201, ApiResponse<T>.Created(data, message));
    }

    /// <summary>
    /// Trả 200 OK với data + pagination metadata.
    /// </summary>
    protected IActionResult PagedResponse<T>(
        T data, int page, int pageSize, int totalCount,
        string message = "Thành công")
    {
        var meta = new PaginatedMeta
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
        return Ok(ApiResponse<T>.Paged(data, meta, message));
    }

    /// <summary>
    /// Trả 204 No Content (cho delete).
    /// </summary>
    protected new IActionResult NoContent()
    {
        return StatusCode(204);
    }
}
