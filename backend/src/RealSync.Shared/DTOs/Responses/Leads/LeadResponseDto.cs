namespace RealSync.Shared.DTOs.Responses.Leads;

public class LeadResponseDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public int Score { get; set; }
    public string LeadTemperature { get; set; } = string.Empty;
    public Guid? InterestedPropertyId { get; set; }
    public decimal? Budget { get; set; }
    public string? Requirements { get; set; }
    public string? PreferredArea { get; set; }
    public string? PreferredType { get; set; }
    public Guid? AssignedToId { get; set; }
    public string? SourceChannel { get; set; }
    public DateTime? LastContactedAt { get; set; }
    public DateTime? NextFollowUpAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
