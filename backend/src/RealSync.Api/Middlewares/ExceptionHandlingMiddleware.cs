using System.Net;
using System.Text.Json;
using RealSync.Shared.DTOs.Responses;
using RealSync.Shared.Exceptions;

namespace RealSync.Api.Middlewares;

/// <summary>
/// Middleware xử lý exception toàn cục.
/// Catch tất cả exception → trả ApiResponse thống nhất.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message, errors) = exception switch
        {
            Shared.Exceptions.ValidationException validationEx =>
                (validationEx.StatusCode, validationEx.Message, (object?)validationEx.Errors),

            AppException appEx =>
                (appEx.StatusCode, appEx.Message, (object?)null),

            _ => (500, "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.", (object?)null)
        };

        // Log error — chỉ log chi tiết cho 500
        if (statusCode >= 500)
        {
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning("Handled exception ({StatusCode}): {Message}", statusCode, exception.Message);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = ApiResponse<object>.Fail(message, statusCode, errors);

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
