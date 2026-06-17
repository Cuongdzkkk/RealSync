using System.Collections.ObjectModel;

namespace RealSync.Shared.Validators.Leads;

public static class LeadValidationRules
{
    private static readonly ISet<string> AllowedStatuses = new HashSet<string>(
        new[] { "New", "Contacted", "Qualified", "Proposal", "Won", "Lost" },
        StringComparer.OrdinalIgnoreCase);

    private static readonly ISet<string> AllowedPriorities = new HashSet<string>(
        new[] { "Low", "Normal", "High", "Urgent" },
        StringComparer.OrdinalIgnoreCase);

    private static readonly ISet<string> AllowedActivityTypes = new HashSet<string>(
        new[] { "Call", "Email", "Meeting", "Note", "StatusChange", "Assigned", "FollowUp", "Converted" },
        StringComparer.OrdinalIgnoreCase);

    private static readonly ISet<string> ClientActivityTypes = new HashSet<string>(
        new[] { "Call", "Email", "Meeting", "Note" },
        StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyCollection<string> Statuses { get; } =
        new ReadOnlyCollection<string>(AllowedStatuses.ToArray());

    public static IReadOnlyCollection<string> Priorities { get; } =
        new ReadOnlyCollection<string>(AllowedPriorities.ToArray());

    public static IReadOnlyCollection<string> ActivityTypes { get; } =
        new ReadOnlyCollection<string>(AllowedActivityTypes.ToArray());

    public static IReadOnlyCollection<string> ClientCreatableActivityTypes { get; } =
        new ReadOnlyCollection<string>(ClientActivityTypes.ToArray());

    public static bool IsValidStatus(string? status)
    {
        return string.IsNullOrWhiteSpace(status) || AllowedStatuses.Contains(status.Trim());
    }

    public static bool IsValidPriority(string? priority)
    {
        return string.IsNullOrWhiteSpace(priority) || AllowedPriorities.Contains(priority.Trim());
    }

    public static bool IsValidActivityType(string? activityType)
    {
        return string.IsNullOrWhiteSpace(activityType) || AllowedActivityTypes.Contains(activityType.Trim());
    }

    public static bool IsClientCreatableActivityType(string? activityType)
    {
        return !string.IsNullOrWhiteSpace(activityType) && ClientActivityTypes.Contains(activityType.Trim());
    }
}
