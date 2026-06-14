using Microsoft.AspNetCore.Mvc;
using RealSync.Api.Filters;
using RealSync.Core.Interfaces;
using RealSync.Shared.DTOs.Requests.Properties;
using RealSync.Shared.DTOs.Responses.Properties;

namespace RealSync.Api.Controllers;

[Route("api/property-categories")]
[Route("api/v1/property-categories")]
public class PropertyCategoriesController : BaseController
{
    private readonly IPropertyService _propertyService;

    public PropertyCategoriesController(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    [HttpGet]
    [RequirePermission("properties.read")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<IReadOnlyList<PropertyCategoryDto>>), 200)]
    public async Task<IActionResult> GetCategories()
    {
        var result = await _propertyService.GetCategoriesAsync();
        return OkResponse(result);
    }

    [HttpPost]
    [RequirePermission("properties.create")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<PropertyCategoryDto>), 201)]
    public async Task<IActionResult> CreateCategory([FromBody] CreatePropertyCategoryDto request)
    {
        var result = await _propertyService.CreateCategoryAsync(request);
        return CreatedResponse(result, "Tạo danh mục bất động sản thành công");
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("properties.update")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<PropertyCategoryDto>), 200)]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdatePropertyCategoryDto request)
    {
        var result = await _propertyService.UpdateCategoryAsync(id, request);
        return OkResponse(result, "Cập nhật danh mục bất động sản thành công");
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("properties.delete")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        await _propertyService.DeleteCategoryAsync(id);
        return NoContent();
    }
}
