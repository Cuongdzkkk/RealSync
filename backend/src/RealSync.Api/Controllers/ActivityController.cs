using Microsoft.AspNetCore.Mvc;
using RealSync.Api.Filters;
using RealSync.Core.Entities;
using RealSync.Core.Interfaces;
using RealSync.Shared.DTOs.Requests;

namespace RealSync.Api.Controllers;

public class ActivityController : BaseController
{
    private readonly IRepository<ActivityLog> _activityRepository;

    public ActivityController(IRepository<ActivityLog> activityRepository)
    {
        _activityRepository = activityRepository;
    }

    [HttpGet]
    [RequirePermission("system.logs")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<IEnumerable<ActivityLog>>), 200)]
    public async Task<IActionResult> GetActivities([FromQuery] PaginationRequest pagination)
    {
        // filter could be applied via search string if needed
        var (items, totalCount) = await _activityRepository.GetPagedAsync(pagination);

        return PagedResponse(items, pagination.Page, pagination.PageSize, totalCount);
    }
}
