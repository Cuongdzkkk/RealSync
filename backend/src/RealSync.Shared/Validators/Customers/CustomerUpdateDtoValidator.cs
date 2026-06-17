using FluentValidation;
using RealSync.Shared.DTOs.Requests.Customers;

namespace RealSync.Shared.Validators.Customers;

public class CustomerUpdateDtoValidator : AbstractValidator<CustomerUpdateDto>
{
    public CustomerUpdateDtoValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Tên khách hàng không được để trống.")
            .MaximumLength(200).WithMessage("Tên khách hàng tối đa 200 ký tự.")
            .When(x => x.FullName != null);

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Số điện thoại tối đa 20 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email không hợp lệ.")
            .MaximumLength(200).WithMessage("Email tối đa 200 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Địa chỉ tối đa 500 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Address));

        RuleFor(x => x.Company)
            .MaximumLength(200).WithMessage("Tên công ty tối đa 200 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Company));

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Ghi chú tối đa 2000 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));

        RuleFor(x => x.Source)
            .MaximumLength(50).WithMessage("Nguồn khách hàng tối đa 50 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Source));
    }
}
