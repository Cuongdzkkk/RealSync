using FluentValidation;
using RealSync.Shared.DTOs.Requests.Leads;

namespace RealSync.Shared.Validators.Leads;

public class LeadQueryDtoValidator : AbstractValidator<LeadQueryDto>
{
    public LeadQueryDtoValidator()
    {
        RuleFor(x => x.Status)
            .Must(LeadValidationRules.IsValidStatus)
            .WithMessage("Trạng thái lead không hợp lệ.");

        RuleFor(x => x.Priority)
            .Must(LeadValidationRules.IsValidPriority)
            .WithMessage("Mức ưu tiên lead không hợp lệ.");

        RuleFor(x => x.MinScore)
            .InclusiveBetween(0, 100).When(x => x.MinScore.HasValue)
            .WithMessage("MinScore phải từ 0 đến 100.");

        RuleFor(x => x.MaxScore)
            .InclusiveBetween(0, 100).When(x => x.MaxScore.HasValue)
            .WithMessage("MaxScore phải từ 0 đến 100.");

        RuleFor(x => x)
            .Must(x => !x.MinScore.HasValue || !x.MaxScore.HasValue || x.MinScore <= x.MaxScore)
            .WithMessage("MinScore không được lớn hơn MaxScore.")
            .OverridePropertyName("score");

        RuleFor(x => x)
            .Must(x => !x.MinBudget.HasValue || !x.MaxBudget.HasValue || x.MinBudget <= x.MaxBudget)
            .WithMessage("MinBudget không được lớn hơn MaxBudget.")
            .OverridePropertyName("budget");
    }
}
