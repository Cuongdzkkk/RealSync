using FluentValidation;
using RealSync.Shared.DTOs.Requests.Properties;

namespace RealSync.Shared.Validators.Properties;

public class CreatePropertyCategoryDtoValidator : AbstractValidator<CreatePropertyCategoryDto>
{
    public CreatePropertyCategoryDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên danh mục không được để trống.")
            .MaximumLength(100).WithMessage("Tên danh mục tối đa 100 ký tự.");

        RuleFor(x => x.Slug)
            .MaximumLength(150).WithMessage("Slug tối đa 150 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Slug));

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Mô tả tối đa 500 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}

public class UpdatePropertyCategoryDtoValidator : AbstractValidator<UpdatePropertyCategoryDto>
{
    public UpdatePropertyCategoryDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên danh mục không được để trống.")
            .MaximumLength(100).WithMessage("Tên danh mục tối đa 100 ký tự.")
            .When(x => x.Name != null);

        RuleFor(x => x.Slug)
            .MaximumLength(150).WithMessage("Slug tối đa 150 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Slug));

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Mô tả tối đa 500 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}
