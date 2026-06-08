namespace RealSync.Shared.DTOs.Requests.Properties;

public class CreatePropertyCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
