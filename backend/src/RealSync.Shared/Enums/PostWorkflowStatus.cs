namespace RealSync.Core.Enums;

/// <summary>
/// Trạng thái của quy trình kiểm duyệt bài viết tổng thể.
/// </summary>
public enum PostWorkflowStatus
{
    Draft = 0,
    InReview = 1,
    Approved = 2,
    Rejected = 3,
    Scheduled = 4,
    Publishing = 5,
    PartiallyPublished = 6,
    Published = 7,
    Failed = 8,
    Cancelled = 9,
    Archived = 10
}
