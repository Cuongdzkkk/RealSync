namespace RealSync.Shared.DTOs.Responses.Properties;

public class PropertyDetailDto : PropertyResponseDto
{
    public string? SourceType { get; set; }
    public string? SourceUrl { get; set; }
    public string? Slug { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public IReadOnlyList<PropertyImageDto> Images { get; set; } = Array.Empty<PropertyImageDto>();
}
