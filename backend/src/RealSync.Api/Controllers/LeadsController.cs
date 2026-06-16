using Microsoft.AspNetCore.Mvc;
using RealSync.Api.Filters;
using RealSync.Core.Interfaces;
using RealSync.Shared.DTOs.Requests.Leads;
using RealSync.Shared.DTOs.Responses.Leads;

namespace RealSync.Api.Controllers;

[Route("api/leads")]
[Route("api/v1/leads")]
public class LeadsController : BaseController
{
    private readonly ILeadService _leadService;

    public LeadsController(ILeadService leadService)
    {
        _leadService = leadService;
    }

    [HttpGet]
    [RequirePermission("leads.read")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<IEnumerable<LeadListItemDto>>), 200)]
    public async Task<IActionResult> GetLeads([FromQuery] LeadQueryDto query)
    {
        var (items, totalCount) = await _leadService.GetLeadsAsync(query);
        return PagedResponse(items, query.Page, query.PageSize, totalCount);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("leads.read")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<LeadDetailDto>), 200)]
    public async Task<IActionResult> GetLead(Guid id)
    {
        var result = await _leadService.GetLeadByIdAsync(id);
        return OkResponse(result);
    }

    [HttpPost]
    [RequirePermission("leads.create")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<LeadResponseDto>), 201)]
    public async Task<IActionResult> CreateLead([FromBody] LeadCreateDto request)
    {
        var result = await _leadService.CreateLeadAsync(request);
        return CreatedResponse(result, "Tạo lead thành công");
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("leads.update")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<LeadResponseDto>), 200)]
    public async Task<IActionResult> UpdateLead(Guid id, [FromBody] LeadUpdateDto request)
    {
        var result = await _leadService.UpdateLeadAsync(id, request);
        return OkResponse(result, "Cập nhật lead thành công");
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("leads.delete")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteLead(Guid id)
    {
        await _leadService.DeleteLeadAsync(id);
        return NoContent();
    }
}
