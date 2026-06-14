using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using RealSync.Core.Entities;
using RealSync.Core.Interfaces;
using RealSync.Shared.DTOs.Requests;
using RealSync.Shared.DTOs.Requests.Properties;
using RealSync.Shared.DTOs.Responses.Properties;
using RealSync.Shared.Exceptions;
using ValidationException = RealSync.Shared.Exceptions.ValidationException;

namespace RealSync.Services.Implementations;

public class PropertyService : IPropertyService
{
    private const long MaxImageSize = 5 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    private static readonly Dictionary<string, string> StatusMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Draft"] = "Draft",
        ["Active"] = "Active",
        ["Available"] = "Available",
        ["Sold"] = "Sold",
        ["Rented"] = "Rented",
        ["Expired"] = "Expired",
        ["Hidden"] = "Hidden"
    };

    private static readonly Dictionary<string, string> ListingTypeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Sale"] = "Sale",
        ["Rent"] = "Rent"
    };

    private readonly IPropertyRepository _propertyRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<PropertyService> _logger;

    public PropertyService(
        IPropertyRepository propertyRepository,
        IFileStorageService fileStorageService,
        ILogger<PropertyService> logger)
    {
        _propertyRepository = propertyRepository;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<(IReadOnlyList<PropertyListItemDto> Items, int TotalCount)> GetPropertiesAsync(PropertyQueryDto query)
    {
        var (items, totalCount) = await _propertyRepository.GetPagedAsync(query);
        return (items.Select(MapListItem).ToList(), totalCount);
    }

    public async Task<PropertyDetailDto> GetPropertyByIdAsync(Guid id)
    {
        var property = await _propertyRepository.GetDetailByIdAsync(id)
            ?? throw new NotFoundException("Property", id);

        return MapDetail(property);
    }

    public async Task<PropertyResponseDto> CreatePropertyAsync(PropertyCreateDto dto)
    {
        await EnsureCategoryExistsAsync(dto.PropertyCategoryId);
        await EnsureTypeExistsAsync(dto.PropertyTypeId);

        var property = new Property
        {
            PropertyCode = await EnsureUniquePropertyCodeAsync(dto.Code),
            Title = dto.Title.Trim(),
            Description = NormalizeOptional(dto.Description),
            Price = dto.Price,
            Area_ = dto.Area,
            PriceUnit = NormalizeOptional(dto.PriceUnit) ?? "VND",
            Address = NormalizeOptional(dto.Address),
            PropertyCategoryId = dto.PropertyCategoryId,
            PropertyTypeId = dto.PropertyTypeId,
            ProjectId = dto.ProjectId,
            Status = NormalizeStatus(dto.Status),
            ListingType = NormalizeListingType(dto.ListingType),
            Bedrooms = dto.Bedrooms ?? 0,
            Bathrooms = dto.Bathrooms ?? 0,
            Floors = dto.Floors ?? 0,
            Direction = NormalizeOptional(dto.Direction),
            LegalStatus = NormalizeOptional(dto.LegalStatus),
            SourceType = "Manual",
            Slug = GenerateSlug(dto.Title)
        };

        await ApplyLocationAsync(property, dto.ProvinceId, dto.DistrictId, dto.WardId);

        await _propertyRepository.CreateAsync(property);

        var detail = await _propertyRepository.GetDetailByIdAsync(property.Id)
            ?? property;

        return MapResponse(detail);
    }

    public async Task<PropertyResponseDto> UpdatePropertyAsync(Guid id, PropertyUpdateDto dto)
    {
        var property = await _propertyRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Property", id);

        if (dto.Title != null)
        {
            property.Title = dto.Title.Trim();
            property.Slug = GenerateSlug(dto.Title);
        }

        if (dto.Description != null)
            property.Description = NormalizeOptional(dto.Description);

        if (dto.Code != null)
            property.PropertyCode = await EnsureUniquePropertyCodeAsync(dto.Code, id);

        if (dto.Price.HasValue)
            property.Price = dto.Price.Value;

        if (dto.Area.HasValue)
            property.Area_ = dto.Area.Value;

        if (dto.PriceUnit != null)
            property.PriceUnit = NormalizeOptional(dto.PriceUnit) ?? "VND";

        if (dto.Address != null)
            property.Address = NormalizeOptional(dto.Address);

        if (dto.PropertyCategoryId.HasValue)
        {
            await EnsureCategoryExistsAsync(dto.PropertyCategoryId.Value);
            property.PropertyCategoryId = dto.PropertyCategoryId.Value;
        }

        if (dto.PropertyTypeId.HasValue)
        {
            await EnsureTypeExistsAsync(dto.PropertyTypeId.Value);
            property.PropertyTypeId = dto.PropertyTypeId.Value;
        }

        if (dto.ProjectId.HasValue)
            property.ProjectId = dto.ProjectId.Value;

        if (dto.Status != null)
            property.Status = NormalizeStatus(dto.Status);

        if (dto.ListingType != null)
            property.ListingType = NormalizeListingType(dto.ListingType);

        if (dto.Bedrooms.HasValue)
            property.Bedrooms = dto.Bedrooms.Value;

        if (dto.Bathrooms.HasValue)
            property.Bathrooms = dto.Bathrooms.Value;

        if (dto.Floors.HasValue)
            property.Floors = dto.Floors.Value;

        if (dto.Direction != null)
            property.Direction = NormalizeOptional(dto.Direction);

        if (dto.LegalStatus != null)
            property.LegalStatus = NormalizeOptional(dto.LegalStatus);

        if (dto.ProvinceId.HasValue || dto.DistrictId.HasValue || dto.WardId.HasValue)
            await ApplyLocationAsync(property, dto.ProvinceId, dto.DistrictId, dto.WardId);

        await _propertyRepository.UpdateAsync(property);

        var detail = await _propertyRepository.GetDetailByIdAsync(id)
            ?? property;

        return MapResponse(detail);
    }

    public async Task DeletePropertyAsync(Guid id)
    {
        var property = await _propertyRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Property", id);

        await _propertyRepository.DeleteAsync(property);
    }

    public async Task<IReadOnlyList<PropertyImageDto>> UploadImagesAsync(Guid propertyId, IReadOnlyCollection<FileUploadRequest> files)
    {
        if (files.Count == 0)
            throw new ValidationException("files", "Vui lòng chọn ít nhất một hình ảnh.");

        var property = await _propertyRepository.GetDetailByIdAsync(propertyId)
            ?? throw new NotFoundException("Property", propertyId);

        var uploadedKeys = new List<string>();
        var images = new List<PropertyImage>();
        var nextSortOrder = property.Images.Any() ? property.Images.Max(i => i.SortOrder) + 1 : 0;
        var hasThumbnail = property.Images.Any(i => i.IsThumbnail || i.IsPrimary);

        try
        {
            foreach (var file in files)
            {
                ValidateImage(file);

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var fileName = $"{Guid.NewGuid():N}{extension}";
                var storageKey = $"properties/{propertyId}/{fileName}";
                var isThumbnail = !hasThumbnail && images.Count == 0;
                var uploadResult = await _fileStorageService.UploadAsync(storageKey, file);
                uploadedKeys.Add(uploadResult.Key);

                images.Add(new PropertyImage
                {
                    PropertyId = propertyId,
                    FileName = fileName,
                    OriginalFileName = Path.GetFileName(file.FileName),
                    FilePath = uploadResult.Key,
                    Url = uploadResult.Url,
                    ContentType = file.ContentType,
                    Size = file.Length,
                    IsThumbnail = isThumbnail,
                    IsPrimary = isThumbnail,
                    SortOrder = nextSortOrder++
                });
            }

            await _propertyRepository.AddImagesAsync(images);
            return images.Select(MapImage).ToList();
        }
        catch
        {
            foreach (var key in uploadedKeys)
                await TryDeleteStoredFileAsync(key);

            throw;
        }
    }

    public async Task DeleteImageAsync(Guid propertyId, Guid imageId)
    {
        var image = await _propertyRepository.GetImageAsync(propertyId, imageId)
            ?? throw new NotFoundException("PropertyImage", imageId);

        await _propertyRepository.DeleteImageAsync(image);
        await TryDeleteStoredFileAsync(image.FilePath);
    }

    public async Task<PropertyImageDto> SetThumbnailAsync(Guid propertyId, Guid imageId)
    {
        if (!await _propertyRepository.ExistsAsync(propertyId))
            throw new NotFoundException("Property", propertyId);

        var image = await _propertyRepository.SetThumbnailAsync(propertyId, imageId)
            ?? throw new NotFoundException("PropertyImage", imageId);

        return MapImage(image);
    }

    public async Task<IReadOnlyList<PropertyCategoryDto>> GetCategoriesAsync()
    {
        var categories = await _propertyRepository.GetCategoriesAsync();
        return categories.Select(MapCategory).ToList();
    }

    public async Task<PropertyCategoryDto> CreateCategoryAsync(CreatePropertyCategoryDto dto)
    {
        var slug = await EnsureUniqueCategorySlugAsync(dto.Slug, dto.Name);
        var category = new PropertyCategory
        {
            Name = dto.Name.Trim(),
            Slug = slug,
            Description = NormalizeOptional(dto.Description),
            IsActive = dto.IsActive
        };

        await _propertyRepository.CreateCategoryAsync(category);
        return MapCategory(category);
    }

    public async Task<PropertyCategoryDto> UpdateCategoryAsync(Guid id, UpdatePropertyCategoryDto dto)
    {
        var category = await _propertyRepository.GetCategoryByIdAsync(id)
            ?? throw new NotFoundException("PropertyCategory", id);

        if (dto.Name != null)
            category.Name = dto.Name.Trim();

        if (dto.Slug != null || dto.Name != null)
            category.Slug = await EnsureUniqueCategorySlugAsync(dto.Slug, category.Name, id);

        if (dto.Description != null)
            category.Description = NormalizeOptional(dto.Description);

        if (dto.IsActive.HasValue)
            category.IsActive = dto.IsActive.Value;

        await _propertyRepository.UpdateCategoryAsync(category);
        return MapCategory(category);
    }

    public async Task DeleteCategoryAsync(Guid id)
    {
        var category = await _propertyRepository.GetCategoryByIdAsync(id)
            ?? throw new NotFoundException("PropertyCategory", id);

        await _propertyRepository.DeleteCategoryAsync(category);
    }

    public async Task<IReadOnlyList<PropertyTypeDto>> GetTypesAsync()
    {
        var types = await _propertyRepository.GetTypesAsync();
        return types.Select(MapType).ToList();
    }

    public async Task<IReadOnlyList<LocationDto>> GetProvincesAsync()
    {
        var provinces = await _propertyRepository.GetAreasAsync(level: 1);
        return provinces.Select(MapLocation).ToList();
    }

    public async Task<IReadOnlyList<LocationDto>> GetDistrictsAsync(Guid provinceId)
    {
        var province = await _propertyRepository.GetAreaByIdAsync(provinceId);
        if (province == null || province.Level != 1)
            throw new NotFoundException("Province", provinceId);

        var districts = await _propertyRepository.GetAreasAsync(level: 2, parentId: provinceId);
        return districts.Select(MapLocation).ToList();
    }

    public async Task<IReadOnlyList<LocationDto>> GetWardsAsync(Guid districtId)
    {
        var district = await _propertyRepository.GetAreaByIdAsync(districtId);
        if (district == null || district.Level != 2)
            throw new NotFoundException("District", districtId);

        var wards = await _propertyRepository.GetAreasAsync(level: 3, parentId: districtId);
        return wards.Select(MapLocation).ToList();
    }

    private async Task EnsureCategoryExistsAsync(Guid categoryId)
    {
        if (!await _propertyRepository.CategoryExistsAsync(categoryId))
            throw new ValidationException("propertyCategoryId", "Danh mục bất động sản không tồn tại hoặc đang bị tắt.");
    }

    private async Task EnsureTypeExistsAsync(Guid typeId)
    {
        if (!await _propertyRepository.TypeExistsAsync(typeId))
            throw new ValidationException("propertyTypeId", "Loại bất động sản không tồn tại hoặc đang bị tắt.");
    }

    private async Task ApplyLocationAsync(Property property, Guid? provinceId, Guid? districtId, Guid? wardId)
    {
        Area? selectedArea;
        Area? province = null;
        Area? district = null;
        Area? ward = null;

        if (wardId.HasValue)
        {
            ward = await _propertyRepository.GetAreaByIdAsync(wardId.Value);
            if (ward == null || ward.Level != 3)
                throw new ValidationException("wardId", "Phường/xã không hợp lệ.");

            district = ward.Parent;
            province = district?.Parent;
            selectedArea = ward;

            if (districtId.HasValue && district?.Id != districtId.Value)
                throw new ValidationException("districtId", "Quận/huyện không khớp với phường/xã.");

            if (provinceId.HasValue && province?.Id != provinceId.Value)
                throw new ValidationException("provinceId", "Tỉnh/thành không khớp với phường/xã.");
        }
        else if (districtId.HasValue)
        {
            district = await _propertyRepository.GetAreaByIdAsync(districtId.Value);
            if (district == null || district.Level != 2)
                throw new ValidationException("districtId", "Quận/huyện không hợp lệ.");

            province = district.Parent;
            selectedArea = district;

            if (provinceId.HasValue && province?.Id != provinceId.Value)
                throw new ValidationException("provinceId", "Tỉnh/thành không khớp với quận/huyện.");
        }
        else if (provinceId.HasValue)
        {
            province = await _propertyRepository.GetAreaByIdAsync(provinceId.Value);
            if (province == null || province.Level != 1)
                throw new ValidationException("provinceId", "Tỉnh/thành không hợp lệ.");

            selectedArea = province;
        }
        else
        {
            return;
        }

        property.AreaId = selectedArea.Id;
        property.Area = selectedArea;
        property.Province = province?.Name;
        property.District = district?.Name;
        property.Ward = ward?.Name;
    }

    private async Task<string> EnsureUniquePropertyCodeAsync(string? requestedCode, Guid? currentPropertyId = null)
    {
        var code = NormalizeOptional(requestedCode);
        if (code == null)
        {
            do
            {
                code = $"RS-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6]}";
            }
            while (await _propertyRepository.GetByCodeAsync(code) != null);

            return code;
        }

        var existing = await _propertyRepository.GetByCodeAsync(code);
        if (existing != null && existing.Id != currentPropertyId)
            throw new ValidationException("code", "Mã bất động sản đã tồn tại.");

        return code;
    }

    private async Task<string> EnsureUniqueCategorySlugAsync(string? requestedSlug, string name, Guid? currentCategoryId = null)
    {
        var baseSlug = GenerateSlug(string.IsNullOrWhiteSpace(requestedSlug) ? name : requestedSlug);
        if (string.IsNullOrWhiteSpace(baseSlug))
            baseSlug = Guid.NewGuid().ToString("N")[..8];

        var slug = baseSlug;
        var suffix = 1;

        while (true)
        {
            var existing = await _propertyRepository.GetCategoryBySlugAsync(slug);
            if (existing == null || existing.Id == currentCategoryId)
                return slug;

            slug = $"{baseSlug}-{suffix++}";
        }
    }

    private static void ValidateImage(FileUploadRequest file)
    {
        if (file.Length <= 0)
            throw new ValidationException("files", $"File '{file.FileName}' không có dữ liệu.");

        if (file.Length > MaxImageSize)
            throw new ValidationException("files", $"File '{file.FileName}' vượt quá giới hạn 5MB.");

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
            throw new ValidationException("files", $"File '{file.FileName}' không đúng định dạng hình ảnh được hỗ trợ.");

        if (!AllowedContentTypes.Contains(file.ContentType))
            throw new ValidationException("files", $"Content-Type của file '{file.FileName}' không hợp lệ.");
    }

    private async Task TryDeleteStoredFileAsync(string key)
    {
        try
        {
            await _fileStorageService.DeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete property image from storage {Key}", key);
        }
    }

    private static PropertyListItemDto MapListItem(Property property)
    {
        var location = ResolveLocation(property.Area);
        var thumbnail = property.Images
            .OrderByDescending(i => i.IsThumbnail || i.IsPrimary)
            .ThenBy(i => i.SortOrder)
            .FirstOrDefault();

        return new PropertyListItemDto
        {
            Id = property.Id,
            Title = property.Title,
            Code = property.PropertyCode,
            Price = property.Price,
            Area = property.Area_,
            Acreage = property.Area_,
            Address = property.Address,
            ProvinceId = location.ProvinceId,
            ProvinceName = location.ProvinceName ?? property.Province,
            DistrictId = location.DistrictId,
            DistrictName = location.DistrictName ?? property.District,
            WardId = location.WardId,
            WardName = location.WardName ?? property.Ward,
            PropertyCategoryId = property.PropertyCategoryId,
            PropertyCategoryName = property.PropertyCategory?.Name,
            PropertyTypeId = property.PropertyTypeId,
            PropertyTypeName = property.PropertyType?.Name,
            Status = property.Status,
            ListingType = property.ListingType,
            Bedrooms = property.Bedrooms,
            Bathrooms = property.Bathrooms,
            Floors = property.Floors,
            ImageUrl = thumbnail?.Url,
            Source = property.SourceType,
            CreatedAt = property.CreatedAt,
            UpdatedAt = property.UpdatedAt
        };
    }

    private static PropertyResponseDto MapResponse(Property property)
    {
        var location = ResolveLocation(property.Area);

        return new PropertyResponseDto
        {
            Id = property.Id,
            Title = property.Title,
            Code = property.PropertyCode,
            Description = property.Description,
            Price = property.Price,
            Area = property.Area_,
            PriceUnit = property.PriceUnit,
            Address = property.Address,
            ProvinceId = location.ProvinceId,
            ProvinceName = location.ProvinceName ?? property.Province,
            DistrictId = location.DistrictId,
            DistrictName = location.DistrictName ?? property.District,
            WardId = location.WardId,
            WardName = location.WardName ?? property.Ward,
            PropertyCategoryId = property.PropertyCategoryId,
            PropertyCategoryName = property.PropertyCategory?.Name,
            PropertyTypeId = property.PropertyTypeId,
            PropertyTypeName = property.PropertyType?.Name,
            ProjectId = property.ProjectId,
            Status = property.Status,
            ListingType = property.ListingType,
            Bedrooms = property.Bedrooms,
            Bathrooms = property.Bathrooms,
            Floors = property.Floors,
            Direction = property.Direction,
            LegalStatus = property.LegalStatus,
            CreatedAt = property.CreatedAt,
            UpdatedAt = property.UpdatedAt
        };
    }

    private static PropertyDetailDto MapDetail(Property property)
    {
        var response = MapResponse(property);

        return new PropertyDetailDto
        {
            Id = response.Id,
            Title = response.Title,
            Code = response.Code,
            Description = response.Description,
            Price = response.Price,
            Area = response.Area,
            PriceUnit = response.PriceUnit,
            Address = response.Address,
            ProvinceId = response.ProvinceId,
            ProvinceName = response.ProvinceName,
            DistrictId = response.DistrictId,
            DistrictName = response.DistrictName,
            WardId = response.WardId,
            WardName = response.WardName,
            PropertyCategoryId = response.PropertyCategoryId,
            PropertyCategoryName = response.PropertyCategoryName,
            PropertyTypeId = response.PropertyTypeId,
            PropertyTypeName = response.PropertyTypeName,
            ProjectId = response.ProjectId,
            Status = response.Status,
            ListingType = response.ListingType,
            Bedrooms = response.Bedrooms,
            Bathrooms = response.Bathrooms,
            Floors = response.Floors,
            Direction = response.Direction,
            LegalStatus = response.LegalStatus,
            CreatedAt = response.CreatedAt,
            UpdatedAt = response.UpdatedAt,
            SourceType = property.SourceType,
            SourceUrl = property.SourceUrl,
            Slug = property.Slug,
            MetaTitle = property.MetaTitle,
            MetaDescription = property.MetaDescription,
            Images = property.Images
                .OrderBy(i => i.SortOrder)
                .Select(MapImage)
                .ToList()
        };
    }

    private static PropertyImageDto MapImage(PropertyImage image)
    {
        return new PropertyImageDto
        {
            Id = image.Id,
            PropertyId = image.PropertyId,
            FileName = image.FileName,
            OriginalFileName = image.OriginalFileName,
            FilePath = image.FilePath,
            Url = image.Url,
            ContentType = image.ContentType,
            Size = image.Size,
            IsThumbnail = image.IsThumbnail || image.IsPrimary,
            SortOrder = image.SortOrder,
            CreatedAt = image.CreatedAt
        };
    }

    private static PropertyCategoryDto MapCategory(PropertyCategory category)
    {
        return new PropertyCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            Description = category.Description,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }

    private static PropertyTypeDto MapType(PropertyType type)
    {
        return new PropertyTypeDto
        {
            Id = type.Id,
            Name = type.Name,
            Slug = type.Slug,
            Description = type.Description,
            Icon = type.Icon,
            SortOrder = type.SortOrder,
            IsActive = type.IsActive
        };
    }

    private static LocationDto MapLocation(Area area)
    {
        return new LocationDto
        {
            Id = area.Id,
            Name = area.Name,
            Code = area.Slug,
            Level = area.Level,
            ParentId = area.ParentId,
            IsActive = area.IsActive
        };
    }

    private static LocationParts ResolveLocation(Area? area)
    {
        if (area == null)
            return new LocationParts(null, null, null, null, null, null);

        return area.Level switch
        {
            1 => new LocationParts(area.Id, area.Name, null, null, null, null),
            2 => new LocationParts(area.Parent?.Id, area.Parent?.Name, area.Id, area.Name, null, null),
            3 => new LocationParts(
                area.Parent?.Parent?.Id,
                area.Parent?.Parent?.Name,
                area.Parent?.Id,
                area.Parent?.Name,
                area.Id,
                area.Name),
            _ => new LocationParts(null, null, null, null, null, null)
        };
    }

    private static string NormalizeStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return "Draft";

        if (StatusMap.TryGetValue(status.Trim(), out var normalized))
            return normalized;

        throw new ValidationException("status", "Trạng thái bất động sản không hợp lệ.");
    }

    private static string NormalizeListingType(string? listingType)
    {
        if (string.IsNullOrWhiteSpace(listingType))
            return "Sale";

        if (ListingTypeMap.TryGetValue(listingType.Trim(), out var normalized))
            return normalized;

        throw new ValidationException("listingType", "Loại tin đăng không hợp lệ.");
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string GenerateSlug(string value)
    {
        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var character in normalized)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(character);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                builder.Append(character);
        }

        var slug = builder
            .ToString()
            .Normalize(NormalizationForm.FormC)
            .Replace("đ", "d")
            .Replace("Đ", "d");

        slug = Regex.Replace(slug, @"[^a-z0-9]+", "-");
        return Regex.Replace(slug, @"(^-|-$)", string.Empty);
    }

    private sealed record LocationParts(
        Guid? ProvinceId,
        string? ProvinceName,
        Guid? DistrictId,
        string? DistrictName,
        Guid? WardId,
        string? WardName);
}
