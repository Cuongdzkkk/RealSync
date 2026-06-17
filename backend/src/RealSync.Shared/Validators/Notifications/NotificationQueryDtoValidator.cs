using FluentValidation;
using RealSync.Shared.DTOs.Requests.Notifications;

namespace RealSync.Shared.Validators.Notifications;

public class NotificationQueryDtoValidator : AbstractValidator<NotificationQueryDto>
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "System",
        "Lead",
        "Property",
        "Task",
        "Assignment"
    };

    public NotificationQueryDtoValidator()
    {
        RuleFor(x => x.Type)
            .Must(BeValidNotificationType)
            .WithMessage("Loại thông báo không hợp lệ.")
            .When(x => !string.IsNullOrWhiteSpace(x.Type));

        RuleFor(x => x)
            .Must(x => !x.FromDate.HasValue || !x.ToDate.HasValue || x.FromDate.Value <= x.ToDate.Value)
            .WithMessage("Từ ngày phải nhỏ hơn hoặc bằng đến ngày.");

        RuleFor(x => x.SortDirection)
            .Must(x => string.Equals(x, "asc", StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(x, "desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Hướng sắp xếp phải là asc hoặc desc.");
    }

    private static bool BeValidNotificationType(string? type)
    {
        return type != null && AllowedTypes.Contains(type.Trim());
    }
}
