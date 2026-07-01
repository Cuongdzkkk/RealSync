namespace RealSync.Shared.Enums;

/// <summary>
/// Trạng thái của Hangfire worker / API operation cho video.
/// </summary>
public enum VideoJobStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4
}
