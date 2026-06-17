using FluentValidation;
using RealSync.Shared.DTOs.Requests.Leads;

namespace RealSync.Shared.Validators.Leads;

public class LeadActivityCreateDtoValidator : AbstractValidator<LeadActivityCreateDto>
{
    public LeadActivityCreateDtoValidator()
    {
        RuleFor(x => x.ActivityType)
            .NotEmpty().WithMessage("Loại hoạt động không được để trống.")
            .Must(LeadValidationRules.IsClientCreatableActivityType)
            .WithMessage("Loại hoạt động lead không hợp lệ.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Ghi chú nội bộ cần có nội dung.")
            .When(x => string.Equals(x.ActivityType, "Note", StringComparison.OrdinalIgnoreCase));

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Mô tả tối đa 2000 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}
