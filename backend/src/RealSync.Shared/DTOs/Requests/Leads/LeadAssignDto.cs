namespace RealSync.Shared.DTOs.Requests.Leads;

public class LeadAssignDto
{
    public Guid AssignedToId { get; set; }
    public string? Note { get; set; }
}
