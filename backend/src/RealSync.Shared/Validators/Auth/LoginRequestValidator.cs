using FluentValidation;
using RealSync.Shared.DTOs.Requests.Auth;

namespace RealSync.Shared.Validators.Auth;

/// <summary>
/// Validator cho LoginRequest.
/// </summary>
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email không được để trống.")
            .EmailAddress().WithMessage("Email không hợp lệ.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mật khẩu không được để trống.");
    }
}
