namespace RealSync.Core.Entities;

/// <summary>
/// Lịch sử tương tác với Lead.
/// </summary>
public class LeadActivity : BaseEntity
{
    public Guid LeadId { get; set; }
    public Lead Lead { get; set; } = null!;

    public string ActivityType { get; set; } = string.Empty;  // Call, Email, Meeting, Note, StatusChange
    public string? Description { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }

    public Guid? PerformedById { get; set; }
    public User? PerformedBy { get; set; }
}
