using Microsoft.AspNetCore.Mvc;
using RealSync.Api.Filters;
using RealSync.Core.Interfaces;
using RealSync.Shared.DTOs.Requests.ActivityLogs;
using RealSync.Shared.DTOs.Responses.ActivityLogs;

namespace RealSync.Api.Controllers;

public class ActivityController : BaseController
{
    private readonly IActivityLogService _activityLogService;

    public ActivityController(IActivityLogService activityLogService)
    {
        _activityLogService = activityLogService;
    }

    [HttpGet]
    [RequirePermission("system.logs")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<IReadOnlyList<ActivityLogDto>>), 200)]
    public async Task<IActionResult> GetActivities([FromQuery] ActivityLogQueryDto query)
    {
        var (items, totalCount) = await _activityLogService.GetActivityLogsAsync(query);

        return PagedResponse(items, query.Page, query.PageSize, totalCount);
    }
}
