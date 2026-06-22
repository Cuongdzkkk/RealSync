namespace RealSync.Shared.DTOs.Requests.Leads;

public class LeadFollowUpDto
{
    public DateTime NextFollowUpAt { get; set; }
    public string? Note { get; set; }
}
