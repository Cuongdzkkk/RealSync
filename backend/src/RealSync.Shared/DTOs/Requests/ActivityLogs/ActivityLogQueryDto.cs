using RealSync.Shared.DTOs.Requests;

namespace RealSync.Shared.DTOs.Requests.ActivityLogs;

public class ActivityLogQueryDto : PaginationRequest
{
    public Guid? UserId { get; set; }
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string? Action { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
