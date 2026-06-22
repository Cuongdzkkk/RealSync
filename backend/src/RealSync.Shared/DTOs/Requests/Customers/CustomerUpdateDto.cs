namespace RealSync.Shared.DTOs.Requests.Customers;

public class CustomerUpdateDto
{
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Company { get; set; }
    public string? Notes { get; set; }
    public string? Source { get; set; }
    public Guid? AssignedToId { get; set; }
}
