using FluentValidation;
using RealSync.Shared.DTOs.Requests.Leads;

namespace RealSync.Shared.Validators.Leads;

public class LeadFollowUpDtoValidator : AbstractValidator<LeadFollowUpDto>
{
    public LeadFollowUpDtoValidator()
    {
        RuleFor(x => x.NextFollowUpAt)
            .NotEmpty().WithMessage("Thời gian chăm sóc tiếp theo là bắt buộc.")
            .GreaterThan(DateTime.UtcNow).WithMessage("Thời gian chăm sóc tiếp theo phải ở tương lai.");

        RuleFor(x => x.Note)
            .MaximumLength(1000).WithMessage("Ghi chú tối đa 1000 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Note));
    }
}
