namespace RealSync.Shared.Validators.Properties;

internal static class PropertyValidationRules
{
    private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Draft",
        "Active",
        "Available",
        "Sold",
        "Rented",
        "Expired",
        "Hidden"
    };

    private static readonly HashSet<string> ValidListingTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Sale",
        "Rent"
    };

    private static readonly HashSet<string> ValidSortColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        "createdAt",
        "price",
        "area",
        "title"
    };

    public static bool IsValidStatus(string? status)
    {
        return string.IsNullOrWhiteSpace(status) || ValidStatuses.Contains(status);
    }

    public static bool IsValidListingType(string? listingType)
    {
        return string.IsNullOrWhiteSpace(listingType) || ValidListingTypes.Contains(listingType);
    }

    public static bool IsValidSortBy(string? sortBy)
    {
        return string.IsNullOrWhiteSpace(sortBy) || ValidSortColumns.Contains(sortBy);
    }
}
