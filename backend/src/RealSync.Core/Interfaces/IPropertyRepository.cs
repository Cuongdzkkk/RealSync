using RealSync.Core.Entities;
using RealSync.Shared.DTOs.Requests.Properties;

namespace RealSync.Core.Interfaces;

public interface IPropertyRepository
{
    Task<(IReadOnlyList<Property> Items, int TotalCount)> GetPagedAsync(PropertyQueryDto query);
    Task<Property?> GetByIdAsync(Guid id);
    Task<Property?> GetDetailByIdAsync(Guid id);
    Task<Property?> GetByCodeAsync(string code);
    Task<Property> CreateAsync(Property property);
    Task UpdateAsync(Property property);
    Task DeleteAsync(Property property);
    Task<bool> ExistsAsync(Guid id);

    Task AddImagesAsync(IEnumerable<PropertyImage> images);
    Task<PropertyImage?> GetImageAsync(Guid propertyId, Guid imageId);
    Task DeleteImageAsync(PropertyImage image);
    Task<PropertyImage?> SetThumbnailAsync(Guid propertyId, Guid imageId);

    Task<IReadOnlyList<PropertyCategory>> GetCategoriesAsync(bool includeInactive = false);
    Task<PropertyCategory?> GetCategoryByIdAsync(Guid id);
    Task<PropertyCategory?> GetCategoryBySlugAsync(string slug);
    Task<PropertyCategory> CreateCategoryAsync(PropertyCategory category);
    Task UpdateCategoryAsync(PropertyCategory category);
    Task DeleteCategoryAsync(PropertyCategory category);
    Task<bool> CategoryExistsAsync(Guid id);

    Task<IReadOnlyList<PropertyType>> GetTypesAsync(bool includeInactive = false);
    Task<PropertyType?> GetTypeByIdAsync(Guid id);
    Task<bool> TypeExistsAsync(Guid id);

    Task<IReadOnlyList<Area>> GetAreasAsync(int level, Guid? parentId = null);
    Task<Area?> GetAreaByIdAsync(Guid id);
}
