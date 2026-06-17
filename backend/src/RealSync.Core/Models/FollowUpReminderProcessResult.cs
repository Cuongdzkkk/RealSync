namespace RealSync.Core.Models;

public class FollowUpReminderProcessResult
{
    public int Scanned { get; set; }
    public int Sent { get; set; }
    public int Skipped { get; set; }
    public int Failed { get; set; }
}
