using FluentValidation;
using RealSync.Shared.DTOs.Requests.Properties;

namespace RealSync.Shared.Validators.Properties;

public class PropertyQueryDtoValidator : AbstractValidator<PropertyQueryDto>
{
    public PropertyQueryDtoValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page phải lớn hơn hoặc bằng 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize phải nằm trong khoảng 1-100.");

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0).WithMessage("MinPrice phải lớn hơn hoặc bằng 0.")
            .When(x => x.MinPrice.HasValue);

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(0).WithMessage("MaxPrice phải lớn hơn hoặc bằng 0.")
            .When(x => x.MaxPrice.HasValue);

        RuleFor(x => x.MinArea)
            .GreaterThanOrEqualTo(0).WithMessage("MinArea phải lớn hơn hoặc bằng 0.")
            .When(x => x.MinArea.HasValue);

        RuleFor(x => x.MaxArea)
            .GreaterThanOrEqualTo(0).WithMessage("MaxArea phải lớn hơn hoặc bằng 0.")
            .When(x => x.MaxArea.HasValue);

        RuleFor(x => x)
            .Must(x => !x.MinPrice.HasValue || !x.MaxPrice.HasValue || x.MinPrice <= x.MaxPrice)
            .WithMessage("MinPrice phải nhỏ hơn hoặc bằng MaxPrice.");

        RuleFor(x => x)
            .Must(x => !x.MinArea.HasValue || !x.MaxArea.HasValue || x.MinArea <= x.MaxArea)
            .WithMessage("MinArea phải nhỏ hơn hoặc bằng MaxArea.");

        RuleFor(x => x.Status)
            .Must(PropertyValidationRules.IsValidStatus)
            .WithMessage("Trạng thái bất động sản không hợp lệ.");

        RuleFor(x => x.SortBy)
            .Must(PropertyValidationRules.IsValidSortBy)
            .WithMessage("SortBy chỉ hỗ trợ createdAt, price, area, title.");

        RuleFor(x => x.SortDirection)
            .Must(x => string.Equals(x, "asc", StringComparison.OrdinalIgnoreCase)
                || string.Equals(x, "desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SortDirection chỉ hỗ trợ asc hoặc desc.");
    }
}
