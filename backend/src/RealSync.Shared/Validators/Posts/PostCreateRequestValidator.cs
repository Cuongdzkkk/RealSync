using FluentValidation;
using RealSync.Shared.DTOs.Requests.Posts;

namespace RealSync.Shared.Validators.Posts;

/// <summary>
/// Validator cho PostCreateRequest.
/// </summary>
public class PostCreateRequestValidator : AbstractValidator<PostCreateRequest>
{
    public PostCreateRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề không được để trống.")
            .MaximumLength(500).WithMessage("Tiêu đề không được vượt quá 500 ký tự.");

        RuleFor(x => x.Summary)
            .MaximumLength(1000).WithMessage("Tóm tắt không được vượt quá 1000 ký tự.")
            .When(x => x.Summary != null);

        RuleFor(x => x.ThumbnailUrl)
            .MaximumLength(1000).WithMessage("URL thumbnail không được vượt quá 1000 ký tự.")
            .When(x => x.ThumbnailUrl != null);
    }
}
