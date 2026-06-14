namespace RealSync.Shared.DTOs.Responses.Properties;

public class PropertyListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal Area { get; set; }
    public decimal Acreage { get; set; }
    public string? Address { get; set; }
    public Guid? ProvinceId { get; set; }
    public string? ProvinceName { get; set; }
    public Guid? DistrictId { get; set; }
    public string? DistrictName { get; set; }
    public Guid? WardId { get; set; }
    public string? WardName { get; set; }
    public Guid? PropertyCategoryId { get; set; }
    public string? PropertyCategoryName { get; set; }
    public Guid PropertyTypeId { get; set; }
    public string? PropertyTypeName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ListingType { get; set; } = string.Empty;
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public int Floors { get; set; }
    public string? ImageUrl { get; set; }
    public string? Source { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
