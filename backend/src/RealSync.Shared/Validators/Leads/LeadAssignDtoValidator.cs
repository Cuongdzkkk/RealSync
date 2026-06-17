using FluentValidation;
using RealSync.Shared.DTOs.Requests.Leads;

namespace RealSync.Shared.Validators.Leads;

public class LeadAssignDtoValidator : AbstractValidator<LeadAssignDto>
{
    public LeadAssignDtoValidator()
    {
        RuleFor(x => x.AssignedToId)
            .NotEmpty().WithMessage("Người phụ trách là bắt buộc.");

        RuleFor(x => x.Note)
            .MaximumLength(1000).WithMessage("Ghi chú tối đa 1000 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Note));
    }
}
