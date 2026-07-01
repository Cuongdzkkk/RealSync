using FluentValidation;
using RealSync.Shared.DTOs.Requests.Publishing;

namespace RealSync.Shared.Validators.Publishing;

public class PublicationJobRequestValidator : AbstractValidator<PublicationJobRequest>
{
    public PublicationJobRequestValidator()
    {
        RuleFor(x => x.PostId).NotEmpty();
        RuleFor(x => x.ContentVariantId).NotEmpty();
        RuleFor(x => x.Priority).GreaterThanOrEqualTo(0);
    }
}
