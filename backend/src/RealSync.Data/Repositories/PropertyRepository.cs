using Microsoft.EntityFrameworkCore;
using RealSync.Core.Entities;
using RealSync.Core.Interfaces;
using RealSync.Data.Context;
using RealSync.Shared.DTOs.Requests.Properties;

namespace RealSync.Data.Repositories;

public class PropertyRepository : IPropertyRepository
{
    private readonly RealSyncDbContext _context;

    public PropertyRepository(RealSyncDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<Property> Items, int TotalCount)> GetPagedAsync(PropertyQueryDto query)
    {
        IQueryable<Property> properties = _context.Properties
            .AsNoTracking()
            .Include(p => p.PropertyCategory)
            .Include(p => p.PropertyType)
            .Include(p => p.Area)
                .ThenInclude(a => a!.Parent)
                .ThenInclude(a => a!.Parent)
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .AsSplitQuery();

        var search = string.IsNullOrWhiteSpace(query.Search) ? query.Keyword : query.Search;
        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            properties = properties.Where(p =>
                p.Title.Contains(keyword) ||
                p.PropertyCode.Contains(keyword) ||
                (p.Address != null && p.Address.Contains(keyword)) ||
                (p.Ward != null && p.Ward.Contains(keyword)) ||
                (p.District != null && p.District.Contains(keyword)) ||
                (p.Province != null && p.Province.Contains(keyword)));
        }

        if (query.MinPrice.HasValue)
            properties = properties.Where(p => p.Price >= query.MinPrice.Value);

        if (query.MaxPrice.HasValue)
            properties = properties.Where(p => p.Price <= query.MaxPrice.Value);

        if (query.MinArea.HasValue)
            properties = properties.Where(p => p.Area_ >= query.MinArea.Value);

        if (query.MaxArea.HasValue)
            properties = properties.Where(p => p.Area_ <= query.MaxArea.Value);

        if (query.CategoryId.HasValue)
            properties = properties.Where(p => p.PropertyCategoryId == query.CategoryId.Value);

        if (query.TypeId.HasValue)
            properties = properties.Where(p => p.PropertyTypeId == query.TypeId.Value);

        if (!string.IsNullOrWhiteSpace(query.Status))
            properties = properties.Where(p => p.Status == query.Status);

        if (query.WardId.HasValue)
        {
            properties = properties.Where(p => p.AreaId == query.WardId.Value);
        }
        else if (query.DistrictId.HasValue)
        {
            properties = properties.Where(p =>
                p.AreaId == query.DistrictId.Value ||
                (p.Area != null && p.Area.ParentId == query.DistrictId.Value));
        }
        else if (query.ProvinceId.HasValue)
        {
            properties = properties.Where(p =>
                p.AreaId == query.ProvinceId.Value ||
                (p.Area != null && p.Area.ParentId == query.ProvinceId.Value) ||
                (p.Area != null && p.Area.Parent != null && p.Area.Parent.ParentId == query.ProvinceId.Value));
        }

        var totalCount = await properties.CountAsync();
        properties = ApplySorting(properties, query.SortBy, query.SortDirection);

        var items = await properties
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Property?> GetByIdAsync(Guid id)
    {
        return await _context.Properties.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Property?> GetDetailByIdAsync(Guid id)
    {
        return await _context.Properties
            .Include(p => p.PropertyCategory)
            .Include(p => p.PropertyType)
            .Include(p => p.Area)
                .ThenInclude(a => a!.Parent)
                .ThenInclude(a => a!.Parent)
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Property?> GetByCodeAsync(string code)
    {
        return await _context.Properties.FirstOrDefaultAsync(p => p.PropertyCode == code);
    }

    public async Task<Property> CreateAsync(Property property)
    {
        await _context.Properties.AddAsync(property);
        await _context.SaveChangesAsync();
        return property;
    }

    public async Task UpdateAsync(Property property)
    {
        _context.Properties.Update(property);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Property property)
    {
        property.IsDeleted = true;
        property.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Properties.AnyAsync(p => p.Id == id);
    }

    public async Task AddImagesAsync(IEnumerable<PropertyImage> images)
    {
        await _context.PropertyImages.AddRangeAsync(images);
        await _context.SaveChangesAsync();
    }

    public async Task<PropertyImage?> GetImageAsync(Guid propertyId, Guid imageId)
    {
        return await _context.PropertyImages
            .FirstOrDefaultAsync(i => i.PropertyId == propertyId && i.Id == imageId);
    }

    public async Task DeleteImageAsync(PropertyImage image)
    {
        image.IsDeleted = true;
        image.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<PropertyImage?> SetThumbnailAsync(Guid propertyId, Guid imageId)
    {
        var images = await _context.PropertyImages
            .Where(i => i.PropertyId == propertyId)
            .ToListAsync();

        var selected = images.FirstOrDefault(i => i.Id == imageId);
        if (selected == null)
            return null;

        foreach (var image in images)
        {
            var isSelected = image.Id == imageId;
            image.IsThumbnail = isSelected;
            image.IsPrimary = isSelected;
        }

        await _context.SaveChangesAsync();
        return selected;
    }

    public async Task<IReadOnlyList<PropertyCategory>> GetCategoriesAsync(bool includeInactive = false)
    {
        return await _context.PropertyCategories
            .AsNoTracking()
            .Where(c => includeInactive || c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<PropertyCategory?> GetCategoryByIdAsync(Guid id)
    {
        return await _context.PropertyCategories.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<PropertyCategory?> GetCategoryBySlugAsync(string slug)
    {
        return await _context.PropertyCategories.FirstOrDefaultAsync(c => c.Slug == slug);
    }

    public async Task<PropertyCategory> CreateCategoryAsync(PropertyCategory category)
    {
        await _context.PropertyCategories.AddAsync(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task UpdateCategoryAsync(PropertyCategory category)
    {
        _context.PropertyCategories.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCategoryAsync(PropertyCategory category)
    {
        category.IsDeleted = true;
        category.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> CategoryExistsAsync(Guid id)
    {
        return await _context.PropertyCategories.AnyAsync(c => c.Id == id && c.IsActive);
    }

    public async Task<IReadOnlyList<PropertyType>> GetTypesAsync(bool includeInactive = false)
    {
        return await _context.PropertyTypes
            .AsNoTracking()
            .Where(t => includeInactive || t.IsActive)
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<PropertyType?> GetTypeByIdAsync(Guid id)
    {
        return await _context.PropertyTypes.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<bool> TypeExistsAsync(Guid id)
    {
        return await _context.PropertyTypes.AnyAsync(t => t.Id == id && t.IsActive);
    }

    public async Task<IReadOnlyList<Area>> GetAreasAsync(int level, Guid? parentId = null)
    {
        return await _context.Areas
            .AsNoTracking()
            .Where(a => a.Level == level && a.IsActive && (!parentId.HasValue || a.ParentId == parentId.Value))
            .OrderBy(a => a.SortOrder)
            .ThenBy(a => a.Name)
            .ToListAsync();
    }

    public async Task<Area?> GetAreaByIdAsync(Guid id)
    {
        return await _context.Areas
            .Include(a => a.Parent)
                .ThenInclude(a => a!.Parent)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    private static IQueryable<Property> ApplySorting(
        IQueryable<Property> query,
        string? sortBy,
        string? sortDirection)
    {
        var ascending = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        return sortBy?.ToLowerInvariant() switch
        {
            "price" => ascending ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price),
            "area" => ascending ? query.OrderBy(p => p.Area_) : query.OrderByDescending(p => p.Area_),
            "title" => ascending ? query.OrderBy(p => p.Title) : query.OrderByDescending(p => p.Title),
            _ => ascending ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt)
        };
    }
}
