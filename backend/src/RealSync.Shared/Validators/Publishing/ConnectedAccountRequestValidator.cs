using FluentValidation;
using RealSync.Shared.DTOs.Requests.Publishing;

namespace RealSync.Shared.Validators.Publishing;

public class ConnectedAccountRequestValidator : AbstractValidator<ConnectedAccountRequest>
{
    public ConnectedAccountRequestValidator()
    {
        RuleFor(x => x.WorkspaceId).NotEmpty();
        RuleFor(x => x.Provider).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ExternalAccountId).NotEmpty().MaximumLength(200);
    }
}
