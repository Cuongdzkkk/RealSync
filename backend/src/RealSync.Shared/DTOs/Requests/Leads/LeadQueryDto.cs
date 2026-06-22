using RealSync.Shared.DTOs.Requests;

namespace RealSync.Shared.DTOs.Requests.Leads;

public class LeadQueryDto : PaginationRequest
{
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? SourceChannel { get; set; }
    public Guid? AssignedToId { get; set; }
    public Guid? InterestedPropertyId { get; set; }
    public int? MinScore { get; set; }
    public int? MaxScore { get; set; }
    public decimal? MinBudget { get; set; }
    public decimal? MaxBudget { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool? OverdueFollowUp { get; set; }
    public DateTime? FollowUpFrom { get; set; }
    public DateTime? FollowUpTo { get; set; }
}
