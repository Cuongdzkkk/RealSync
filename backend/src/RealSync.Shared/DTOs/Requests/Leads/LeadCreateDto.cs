namespace RealSync.Shared.DTOs.Requests.Leads;

public class LeadCreateDto
{
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public int? Score { get; set; }
    public Guid? InterestedPropertyId { get; set; }
    public decimal? Budget { get; set; }
    public string? Requirements { get; set; }
    public string? PreferredArea { get; set; }
    public string? PreferredType { get; set; }
    public Guid? AssignedToId { get; set; }
    public string? SourceChannel { get; set; }
    public DateTime? LastContactedAt { get; set; }
    public DateTime? NextFollowUpAt { get; set; }
}
