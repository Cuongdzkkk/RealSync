using FluentValidation;
using RealSync.Shared.DTOs.Requests.Properties;

namespace RealSync.Shared.Validators.Properties;

public class PropertyCreateDtoValidator : AbstractValidator<PropertyCreateDto>
{
    public PropertyCreateDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề không được để trống.")
            .MaximumLength(200).WithMessage("Tiêu đề tối đa 200 ký tự.");

        RuleFor(x => x.Description)
            .MaximumLength(4000).WithMessage("Mô tả tối đa 4000 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.Code)
            .MaximumLength(50).WithMessage("Mã bất động sản tối đa 50 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Code));

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Giá phải lớn hơn hoặc bằng 0.");

        RuleFor(x => x.Area)
            .GreaterThanOrEqualTo(0).WithMessage("Diện tích phải lớn hơn hoặc bằng 0.");

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Địa chỉ tối đa 500 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Address));

        RuleFor(x => x.PropertyCategoryId)
            .NotEmpty().WithMessage("Danh mục bất động sản là bắt buộc.");

        RuleFor(x => x.PropertyTypeId)
            .NotEmpty().WithMessage("Loại bất động sản là bắt buộc.");

        RuleFor(x => x.Status)
            .Must(PropertyValidationRules.IsValidStatus)
            .WithMessage("Trạng thái bất động sản không hợp lệ.");

        RuleFor(x => x.ListingType)
            .Must(PropertyValidationRules.IsValidListingType)
            .WithMessage("Loại tin đăng không hợp lệ.");

        RuleFor(x => x.Bedrooms)
            .GreaterThanOrEqualTo(0).WithMessage("Số phòng ngủ phải lớn hơn hoặc bằng 0.")
            .When(x => x.Bedrooms.HasValue);

        RuleFor(x => x.Bathrooms)
            .GreaterThanOrEqualTo(0).WithMessage("Số phòng tắm phải lớn hơn hoặc bằng 0.")
            .When(x => x.Bathrooms.HasValue);

        RuleFor(x => x.Floors)
            .GreaterThanOrEqualTo(0).WithMessage("Số tầng phải lớn hơn hoặc bằng 0.")
            .When(x => x.Floors.HasValue);

        RuleFor(x => x.Direction)
            .MaximumLength(20).WithMessage("Hướng tối đa 20 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Direction));

        RuleFor(x => x.LegalStatus)
            .MaximumLength(100).WithMessage("Pháp lý tối đa 100 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.LegalStatus));
    }
}
