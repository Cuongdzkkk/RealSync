namespace RealSync.Shared.DTOs.Requests.Leads;

public class LeadStatusUpdateDto
{
    public string Status { get; set; } = string.Empty;
    public string? Note { get; set; }
}
