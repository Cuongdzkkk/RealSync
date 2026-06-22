namespace RealSync.Core.Entities;

/// <summary>
/// Records that a follow-up reminder was dispatched for a lead at a specific scheduled time.
/// </summary>
public class FollowUpReminderDispatch : BaseEntity
{
    public Guid LeadId { get; set; }
    public Lead Lead { get; set; } = null!;

    public DateTime ScheduledFor { get; set; }

    public Guid NotificationId { get; set; }
    public Notification Notification { get; set; } = null!;

    public DateTime SentAt { get; set; }
}
