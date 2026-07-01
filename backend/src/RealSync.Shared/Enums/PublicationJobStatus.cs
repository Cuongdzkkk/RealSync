namespace RealSync.Core.Enums;

/// <summary>
/// Trạng thái của một Publication Job.
/// </summary>
public enum PublicationJobStatus
{
    Pending = 0,
    Validating = 1,
    Queued = 2,
    Publishing = 3,
    RemoteProcessing = 4,
    Published = 5,
    RetryScheduled = 6,
    NeedsReview = 7,
    Failed = 8,
    Cancelled = 9
}
