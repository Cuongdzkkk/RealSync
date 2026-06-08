namespace RealSync.Shared.DTOs.Responses.Properties;

public class LocationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public int Level { get; set; }
    public Guid? ParentId { get; set; }
    public bool IsActive { get; set; }
}
