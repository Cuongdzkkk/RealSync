using Microsoft.AspNetCore.Mvc;
using RealSync.Api.Filters;
using RealSync.Core.Interfaces;
using RealSync.Shared.DTOs.Responses.Properties;

namespace RealSync.Api.Controllers;

[Route("api/locations")]
[Route("api/v1/locations")]
public class LocationsController : BaseController
{
    private readonly IPropertyService _propertyService;

    public LocationsController(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    [HttpGet("provinces")]
    [RequirePermission("properties.read")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<IReadOnlyList<LocationDto>>), 200)]
    public async Task<IActionResult> GetProvinces()
    {
        var result = await _propertyService.GetProvincesAsync();
        return OkResponse(result);
    }

    [HttpGet("districts")]
    [RequirePermission("properties.read")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<IReadOnlyList<LocationDto>>), 200)]
    public async Task<IActionResult> GetDistricts([FromQuery] Guid provinceId)
    {
        var result = await _propertyService.GetDistrictsAsync(provinceId);
        return OkResponse(result);
    }

    [HttpGet("wards")]
    [RequirePermission("properties.read")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<IReadOnlyList<LocationDto>>), 200)]
    public async Task<IActionResult> GetWards([FromQuery] Guid districtId)
    {
        var result = await _propertyService.GetWardsAsync(districtId);
        return OkResponse(result);
    }
}
