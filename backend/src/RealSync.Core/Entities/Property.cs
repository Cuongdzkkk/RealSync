namespace RealSync.Core.Entities;

/// <summary>
/// Bất động sản — entity chính của hệ thống.
/// Schema theo database-guide.md Section 4.1.
/// </summary>
public class Property : BaseEntity
{
    // Basic Info
    public string PropertyCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Type & Project
    public Guid? PropertyCategoryId { get; set; }
    public PropertyCategory? PropertyCategory { get; set; }
    public Guid PropertyTypeId { get; set; }
    public PropertyType PropertyType { get; set; } = null!;
    public Guid? ProjectId { get; set; }
    public Project? Project { get; set; }

    // Location
    public Guid? AreaId { get; set; }
    public Area? Area { get; set; }
    public string? Address { get; set; }
    public string? Ward { get; set; }
    public string? District { get; set; }
    public string? Province { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    // Specs
    public decimal Area_ { get; set; }  // Diện tích (m²) — tên tránh conflict với navigation
    public decimal Price { get; set; }
    public string? PriceUnit { get; set; } = "VND";
    public int Bedrooms { get; set; } = 0;
    public int Bathrooms { get; set; } = 0;
    public int Floors { get; set; } = 1;
    public string? Direction { get; set; }
    public string? LegalStatus { get; set; }

    // Status
    public string Status { get; set; } = "Draft";  // Draft, Active, Sold, Rented, Expired
    public string ListingType { get; set; } = "Sale";  // Sale, Rent
    public int FeaturedLevel { get; set; } = 0;  // 0=Normal, 1=Featured, 2=VIP

    // Source
    public string? SourceType { get; set; }  // Manual, Crawled, Imported
    public string? SourceUrl { get; set; }
    public Guid? CrawlJobId { get; set; }
    public CrawlJob? CrawlJob { get; set; }

    // SEO
    public string? Slug { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }

    // Navigation
    public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
}
