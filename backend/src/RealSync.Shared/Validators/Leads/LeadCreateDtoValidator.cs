using FluentValidation;
using RealSync.Shared.DTOs.Requests.Leads;

namespace RealSync.Shared.Validators.Leads;

public class LeadCreateDtoValidator : AbstractValidator<LeadCreateDto>
{
    public LeadCreateDtoValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Tên lead không được để trống.")
            .MaximumLength(200).WithMessage("Tên lead tối đa 200 ký tự.");

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Phone) || !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("Vui lòng nhập số điện thoại hoặc email.")
            .OverridePropertyName("contact");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Số điện thoại tối đa 20 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email không hợp lệ.")
            .MaximumLength(200).WithMessage("Email tối đa 200 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Status)
            .Must(LeadValidationRules.IsValidStatus)
            .WithMessage("Trạng thái lead không hợp lệ.");

        RuleFor(x => x.Priority)
            .Must(LeadValidationRules.IsValidPriority)
            .WithMessage("Mức ưu tiên lead không hợp lệ.");

        RuleFor(x => x.Score)
            .InclusiveBetween(0, 100).WithMessage("Điểm lead phải từ 0 đến 100.")
            .When(x => x.Score.HasValue);

        RuleFor(x => x.Budget)
            .GreaterThanOrEqualTo(0).WithMessage("Ngân sách phải lớn hơn hoặc bằng 0.")
            .When(x => x.Budget.HasValue);
    }
}
