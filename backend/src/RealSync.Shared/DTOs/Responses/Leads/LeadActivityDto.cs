namespace RealSync.Shared.DTOs.Responses.Leads;

public class LeadActivityDto
{
    public Guid Id { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public Guid? PerformedById { get; set; }
    public string? PerformedByName { get; set; }
    public DateTime CreatedAt { get; set; }
}
