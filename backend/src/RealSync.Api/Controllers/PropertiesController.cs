using Microsoft.AspNetCore.Mvc;
using RealSync.Api.Filters;
using RealSync.Core.Interfaces;
using RealSync.Shared.DTOs.Requests;
using RealSync.Shared.DTOs.Requests.Properties;
using RealSync.Shared.DTOs.Responses.Properties;

namespace RealSync.Api.Controllers;

[Route("api/properties")]
[Route("api/v1/properties")]
public class PropertiesController : BaseController
{
    private readonly IPropertyService _propertyService;

    public PropertiesController(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    [HttpGet]
    [RequirePermission("properties.read")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<IEnumerable<PropertyListItemDto>>), 200)]
    public async Task<IActionResult> GetProperties([FromQuery] PropertyQueryDto query)
    {
        var (items, totalCount) = await _propertyService.GetPropertiesAsync(query);
        return PagedResponse(items, query.Page, query.PageSize, totalCount);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("properties.read")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<PropertyDetailDto>), 200)]
    public async Task<IActionResult> GetProperty(Guid id)
    {
        var result = await _propertyService.GetPropertyByIdAsync(id);
        return OkResponse(result);
    }

    [HttpPost]
    [RequirePermission("properties.create")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<PropertyResponseDto>), 201)]
    public async Task<IActionResult> CreateProperty([FromBody] PropertyCreateDto request)
    {
        var result = await _propertyService.CreatePropertyAsync(request);
        return CreatedResponse(result, "Tạo bất động sản thành công");
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("properties.update")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<PropertyResponseDto>), 200)]
    public async Task<IActionResult> UpdateProperty(Guid id, [FromBody] PropertyUpdateDto request)
    {
        var result = await _propertyService.UpdatePropertyAsync(id, request);
        return OkResponse(result, "Cập nhật bất động sản thành công");
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("properties.delete")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteProperty(Guid id)
    {
        await _propertyService.DeletePropertyAsync(id);
        return NoContent();
    }

    [HttpPost("{id:guid}/images")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [RequirePermission("properties.update")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<IReadOnlyList<PropertyImageDto>>), 201)]
    public async Task<IActionResult> UploadImages(Guid id, [FromForm] List<IFormFile>? files)
    {
        var uploadRequests = (files ?? new List<IFormFile>())
            .Select(file => new FileUploadRequest
            {
                FileName = file.FileName,
                ContentType = file.ContentType,
                Length = file.Length,
                OpenReadStream = file.OpenReadStream
            })
            .ToList();

        var result = await _propertyService.UploadImagesAsync(id, uploadRequests);
        return CreatedResponse(result, "Upload hình ảnh thành công");
    }

    [HttpDelete("{propertyId:guid}/images/{imageId:guid}")]
    [RequirePermission("properties.update")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteImage(Guid propertyId, Guid imageId)
    {
        await _propertyService.DeleteImageAsync(propertyId, imageId);
        return NoContent();
    }

    [HttpPatch("{propertyId:guid}/images/{imageId:guid}/thumbnail")]
    [RequirePermission("properties.update")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<PropertyImageDto>), 200)]
    public async Task<IActionResult> SetThumbnail(Guid propertyId, Guid imageId)
    {
        var result = await _propertyService.SetThumbnailAsync(propertyId, imageId);
        return OkResponse(result, "Đã cập nhật ảnh đại diện");
    }
}
