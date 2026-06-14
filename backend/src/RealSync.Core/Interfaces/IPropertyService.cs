using RealSync.Shared.DTOs.Requests;
using RealSync.Shared.DTOs.Requests.Properties;
using RealSync.Shared.DTOs.Responses.Properties;

namespace RealSync.Core.Interfaces;

public interface IPropertyService
{
    Task<(IReadOnlyList<PropertyListItemDto> Items, int TotalCount)> GetPropertiesAsync(PropertyQueryDto query);
    Task<PropertyDetailDto> GetPropertyByIdAsync(Guid id);
    Task<PropertyResponseDto> CreatePropertyAsync(PropertyCreateDto dto);
    Task<PropertyResponseDto> UpdatePropertyAsync(Guid id, PropertyUpdateDto dto);
    Task DeletePropertyAsync(Guid id);
    Task<IReadOnlyList<PropertyImageDto>> UploadImagesAsync(Guid propertyId, IReadOnlyCollection<FileUploadRequest> files);
    Task DeleteImageAsync(Guid propertyId, Guid imageId);
    Task<PropertyImageDto> SetThumbnailAsync(Guid propertyId, Guid imageId);

    Task<IReadOnlyList<PropertyCategoryDto>> GetCategoriesAsync();
    Task<PropertyCategoryDto> CreateCategoryAsync(CreatePropertyCategoryDto dto);
    Task<PropertyCategoryDto> UpdateCategoryAsync(Guid id, UpdatePropertyCategoryDto dto);
    Task DeleteCategoryAsync(Guid id);

    Task<IReadOnlyList<PropertyTypeDto>> GetTypesAsync();
    Task<IReadOnlyList<LocationDto>> GetProvincesAsync();
    Task<IReadOnlyList<LocationDto>> GetDistrictsAsync(Guid provinceId);
    Task<IReadOnlyList<LocationDto>> GetWardsAsync(Guid districtId);
}
