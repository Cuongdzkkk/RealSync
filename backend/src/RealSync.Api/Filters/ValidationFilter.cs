using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RealSync.Shared.DTOs.Responses;

namespace RealSync.Api.Filters;

/// <summary>
/// Action filter tự động validate request body bằng FluentValidation.
/// Trả 422 nếu validation fail — format theo ApiResponse.
/// </summary>
public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Tìm tất cả arguments có validator đã đăng ký
        foreach (var (key, value) in context.ActionArguments)
        {
            if (value == null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(value.GetType());
            var validator = context.HttpContext.RequestServices.GetService(validatorType) as IValidator;

            if (validator == null) continue;

            var validationContext = new ValidationContext<object>(value);
            var result = await validator.ValidateAsync(validationContext);

            if (!result.IsValid)
            {
                var errors = result.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray());

                var response = ApiResponse<object>.Fail(
                    "Dữ liệu không hợp lệ.", 422, errors);

                context.Result = new ObjectResult(response)
                {
                    StatusCode = 422
                };
                return;
            }
        }

        await next();
    }
}
