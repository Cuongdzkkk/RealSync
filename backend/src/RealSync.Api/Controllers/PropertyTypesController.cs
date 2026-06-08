using Microsoft.AspNetCore.Mvc;
using RealSync.Api.Filters;
using RealSync.Core.Interfaces;
using RealSync.Shared.DTOs.Responses.Properties;

namespace RealSync.Api.Controllers;

[Route("api/property-types")]
[Route("api/v1/property-types")]
public class PropertyTypesController : BaseController
{
    private readonly IPropertyService _propertyService;

    public PropertyTypesController(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    [HttpGet]
    [RequirePermission("properties.read")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<IReadOnlyList<PropertyTypeDto>>), 200)]
    public async Task<IActionResult> GetTypes()
    {
        var result = await _propertyService.GetTypesAsync();
        return OkResponse(result);
    }
}
