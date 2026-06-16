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

    public static IReadOnlyCollection<string> Statuses { get; } =
        new ReadOnlyCollection<string>(AllowedStatuses.ToArray());

    public static IReadOnlyCollection<string> Priorities { get; } =
        new ReadOnlyCollection<string>(AllowedPriorities.ToArray());

    public static bool IsValidStatus(string? status)
    {
        return string.IsNullOrWhiteSpace(status) || AllowedStatuses.Contains(status.Trim());
    }

    public static bool IsValidPriority(string? priority)
    {
        return string.IsNullOrWhiteSpace(priority) || AllowedPriorities.Contains(priority.Trim());
    }
}
