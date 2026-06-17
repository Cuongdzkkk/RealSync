using Microsoft.AspNetCore.Mvc;
using RealSync.Api.Filters;
using RealSync.Core.Interfaces;
using RealSync.Shared.DTOs.Requests.CrmAnalytics;
using RealSync.Shared.DTOs.Responses.CrmAnalytics;

namespace RealSync.Api.Controllers;

[Route("api/crm/analytics")]
[Route("api/v1/crm/analytics")]
public class CrmAnalyticsController : BaseController
{
    private readonly ICrmAnalyticsService _crmAnalyticsService;

    public CrmAnalyticsController(ICrmAnalyticsService crmAnalyticsService)
    {
        _crmAnalyticsService = crmAnalyticsService;
    }

    [HttpGet("summary")]
    [RequirePermission("dashboard.analytics")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<CrmAnalyticsSummaryDto>), 200)]
    public async Task<IActionResult> GetSummary([FromQuery] CrmAnalyticsQueryDto query)
    {
        var result = await _crmAnalyticsService.GetSummaryAsync(query);
        return OkResponse(result);
    }

    [HttpGet("leads-by-status")]
    [RequirePermission("dashboard.analytics")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<IReadOnlyList<CrmCountByLabelDto>>), 200)]
    public async Task<IActionResult> GetLeadsByStatus([FromQuery] CrmAnalyticsQueryDto query)
    {
        var result = await _crmAnalyticsService.GetLeadsByStatusAsync(query);
        return OkResponse(result);
    }

    [HttpGet("leads-by-source")]
    [RequirePermission("dashboard.analytics")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<IReadOnlyList<CrmCountByLabelDto>>), 200)]
    public async Task<IActionResult> GetLeadsBySource([FromQuery] CrmAnalyticsQueryDto query)
    {
        var result = await _crmAnalyticsService.GetLeadsBySourceAsync(query);
        return OkResponse(result);
    }

    [HttpGet("conversion")]
    [RequirePermission("dashboard.analytics")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<CrmConversionStatsDto>), 200)]
    public async Task<IActionResult> GetConversion([FromQuery] CrmAnalyticsQueryDto query)
    {
        var result = await _crmAnalyticsService.GetConversionStatsAsync(query);
        return OkResponse(result);
    }

    [HttpGet("follow-ups")]
    [RequirePermission("dashboard.analytics")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<CrmFollowUpStatsDto>), 200)]
    public async Task<IActionResult> GetFollowUps([FromQuery] CrmAnalyticsQueryDto query)
    {
        var result = await _crmAnalyticsService.GetFollowUpStatsAsync(query);
        return OkResponse(result);
    }

    [HttpGet("customers")]
    [RequirePermission("dashboard.analytics")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<CrmCustomerStatsDto>), 200)]
    public async Task<IActionResult> GetCustomers([FromQuery] CrmAnalyticsQueryDto query)
    {
        var result = await _crmAnalyticsService.GetCustomerStatsAsync(query);
        return OkResponse(result);
    }

    [HttpGet("monthly-trend")]
    [RequirePermission("dashboard.analytics")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<IReadOnlyList<CrmMonthlyTrendDto>>), 200)]
    public async Task<IActionResult> GetMonthlyTrend([FromQuery] int? year)
    {
        var result = await _crmAnalyticsService.GetMonthlyTrendAsync(year);
        return OkResponse(result);
    }
}
