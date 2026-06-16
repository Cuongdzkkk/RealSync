using FluentValidation;
using RealSync.Shared.DTOs.Requests.Posts;

namespace RealSync.Shared.Validators.Posts;

/// <summary>
/// Validator cho PostStatusUpdateRequest.
/// </summary>
public class PostStatusUpdateRequestValidator : AbstractValidator<PostStatusUpdateRequest>
{
    private static readonly string[] ValidStatuses =
        { "Draft", "Scheduled", "Published", "Failed", "Archived" };

    public PostStatusUpdateRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Trạng thái không được để trống.")
            .Must(s => ValidStatuses.Contains(s))
            .WithMessage($"Trạng thái phải là một trong: {string.Join(", ", ValidStatuses)}.");
    }
}
