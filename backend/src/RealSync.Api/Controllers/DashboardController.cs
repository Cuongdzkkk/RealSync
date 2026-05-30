using Microsoft.AspNetCore.Mvc;
using RealSync.Api.Filters;
using RealSync.Core.Interfaces;
using RealSync.Shared.DTOs.Responses.Dashboard;

namespace RealSync.Api.Controllers;

public class DashboardController : BaseController
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("statistics")]
    [RequirePermission("dashboard.view")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<DashboardStatisticsResponse>), 200)]
    public async Task<IActionResult> GetStatistics()
    {
        var result = await _dashboardService.GetStatisticsAsync();
        return OkResponse(result);
    }

    [HttpGet("chart/monthly-leads")]
    [RequirePermission("dashboard.analytics")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<IEnumerable<MonthlyStatItem>>), 200)]
    public async Task<IActionResult> GetMonthlyLeads()
    {
        var result = await _dashboardService.GetMonthlyLeadsAsync();
        return OkResponse(result);
    }

    [HttpGet("chart/properties-by-status")]
    [RequirePermission("dashboard.analytics")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<IEnumerable<PropertyStatusStatItem>>), 200)]
    public async Task<IActionResult> GetPropertiesByStatus()
    {
        var result = await _dashboardService.GetPropertiesByStatusAsync();
        return OkResponse(result);
    }
}
