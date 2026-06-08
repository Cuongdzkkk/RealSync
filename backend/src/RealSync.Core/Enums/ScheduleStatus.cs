namespace RealSync.Core.Enums;

/// <summary>
/// Trạng thái lịch đăng bài.
/// </summary>
public enum ScheduleStatus
{
    Pending = 0,
    Executing = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4
}
