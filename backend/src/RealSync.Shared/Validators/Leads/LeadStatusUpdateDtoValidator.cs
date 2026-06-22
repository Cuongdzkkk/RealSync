using FluentValidation;
using RealSync.Shared.DTOs.Requests.Leads;

namespace RealSync.Shared.Validators.Leads;

public class LeadStatusUpdateDtoValidator : AbstractValidator<LeadStatusUpdateDto>
{
    public LeadStatusUpdateDtoValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Trạng thái lead không được để trống.")
            .Must(LeadValidationRules.IsValidStatus)
            .WithMessage("Trạng thái lead không hợp lệ.");

        RuleFor(x => x.Note)
            .MaximumLength(1000).WithMessage("Ghi chú tối đa 1000 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Note));
    }
}
