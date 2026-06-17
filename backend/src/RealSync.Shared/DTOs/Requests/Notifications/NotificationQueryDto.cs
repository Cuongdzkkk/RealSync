using RealSync.Shared.DTOs.Requests;

namespace RealSync.Shared.DTOs.Requests.Notifications;

public class NotificationQueryDto : PaginationRequest
{
    public bool? IsRead { get; set; }
    public string? Type { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
