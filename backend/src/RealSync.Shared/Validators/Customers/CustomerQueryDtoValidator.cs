using FluentValidation;
using RealSync.Shared.DTOs.Requests.Customers;

namespace RealSync.Shared.Validators.Customers;

public class CustomerQueryDtoValidator : AbstractValidator<CustomerQueryDto>
{
    public CustomerQueryDtoValidator()
    {
        RuleFor(x => x)
            .Must(x => !x.FromDate.HasValue || !x.ToDate.HasValue || x.FromDate <= x.ToDate)
            .WithMessage("FromDate không được lớn hơn ToDate.")
            .OverridePropertyName("dateRange");
    }
}
