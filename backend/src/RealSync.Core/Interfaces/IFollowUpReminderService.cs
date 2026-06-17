using RealSync.Core.Models;

namespace RealSync.Core.Interfaces;

public interface IFollowUpReminderService
{
    Task<FollowUpReminderProcessResult> ProcessDueRemindersAsync(CancellationToken cancellationToken = default);
}
