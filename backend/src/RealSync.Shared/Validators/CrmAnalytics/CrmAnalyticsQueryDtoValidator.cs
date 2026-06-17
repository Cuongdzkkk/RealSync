using FluentValidation;
using RealSync.Shared.DTOs.Requests.CrmAnalytics;

namespace RealSync.Shared.Validators.CrmAnalytics;

public class CrmAnalyticsQueryDtoValidator : AbstractValidator<CrmAnalyticsQueryDto>
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "New",
        "Contacted",
        "Qualified",
        "Proposal",
        "Won",
        "Lost"
    };

    public CrmAnalyticsQueryDtoValidator()
    {
        RuleFor(x => x)
            .Must(x => !x.FromDate.HasValue || !x.ToDate.HasValue || x.FromDate.Value <= x.ToDate.Value)
            .WithMessage("Từ ngày phải nhỏ hơn hoặc bằng đến ngày.");

        RuleFor(x => x.Status)
            .Must(status => status != null && AllowedStatuses.Contains(status.Trim()))
            .WithMessage("Trạng thái lead không hợp lệ.")
            .When(x => !string.IsNullOrWhiteSpace(x.Status));
    }
}
