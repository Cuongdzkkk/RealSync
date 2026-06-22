using RealSync.Shared.DTOs.Requests;

namespace RealSync.Shared.DTOs.Requests.Customers;

public class CustomerQueryDto : PaginationRequest
{
    public string? Source { get; set; }
    public Guid? AssignedToId { get; set; }
    public Guid? ConvertedFromLeadId { get; set; }
    public bool? ConvertedFromLead { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
