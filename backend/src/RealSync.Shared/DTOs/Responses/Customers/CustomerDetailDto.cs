namespace RealSync.Shared.DTOs.Responses.Customers;

public class CustomerDetailDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Company { get; set; }
    public string? Notes { get; set; }
    public string? Source { get; set; }
    public Guid? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    public Guid? ConvertedFromLeadId { get; set; }
    public string? ConvertedFromLeadName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public IReadOnlyList<CustomerActivityLogDto> Activities { get; set; } = Array.Empty<CustomerActivityLogDto>();
}
