namespace RealSync.Core.Entities;

/// <summary>
/// Dự án bất động sản.
/// </summary>
public class Project : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Slug { get; set; }
    public string? DeveloperName { get; set; }
    public string? Address { get; set; }
    public string? Status { get; set; } = "Active";  // Active, Completed, Upcoming
    public decimal? TotalArea { get; set; }
    public int? TotalUnits { get; set; }
    public string? ImageUrl { get; set; }

    // Location
    public Guid? AreaId { get; set; }
    public Area? Area { get; set; }

    // Navigation
    public ICollection<Property> Properties { get; set; } = new List<Property>();
}
