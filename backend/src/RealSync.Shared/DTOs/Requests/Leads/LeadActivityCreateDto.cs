namespace RealSync.Shared.DTOs.Requests.Leads;

public class LeadActivityCreateDto
{
    public string ActivityType { get; set; } = string.Empty;
    public string? Description { get; set; }
}
