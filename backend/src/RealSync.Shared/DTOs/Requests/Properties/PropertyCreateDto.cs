namespace RealSync.Shared.DTOs.Requests.Properties;

public class PropertyCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Code { get; set; }
    public decimal Price { get; set; }
    public decimal Area { get; set; }
    public string? PriceUnit { get; set; } = "VND";
    public string? Address { get; set; }
    public Guid? ProvinceId { get; set; }
    public Guid? DistrictId { get; set; }
    public Guid? WardId { get; set; }
    public Guid PropertyCategoryId { get; set; }
    public Guid PropertyTypeId { get; set; }
    public Guid? ProjectId { get; set; }
    public string Status { get; set; } = "Draft";
    public string ListingType { get; set; } = "Sale";
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public int? Floors { get; set; }
    public string? Direction { get; set; }
    public string? LegalStatus { get; set; }
}
