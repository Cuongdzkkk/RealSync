using Microsoft.EntityFrameworkCore;
using RealSync.Core.Enums;
using RealSync.Core.Interfaces;
using RealSync.Data.Context;

namespace RealSync.Api.Middlewares;

public class ApiActivityAuditMiddleware
{
    private static readonly HashSet<string> AuditedMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        HttpMethods.Post,
        HttpMethods.Put,
        HttpMethods.Patch,
        HttpMethods.Delete
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ApiActivityAuditMiddleware> _logger;

    public ApiActivityAuditMiddleware(RequestDelegate next, ILogger<ApiActivityAuditMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        RealSyncDbContext dbContext,
        IActivityLogService activityLogService,
        ICurrentUserService currentUser)
    {
        var startedAt = DateTime.UtcNow;
        await _next(context);

        if (!ShouldAudit(context, currentUser))
            return;

        var userId = currentUser.UserId;
        if (userId.HasValue && await dbContext.ActivityLogs.AnyAsync(a =>
                a.UserId == userId.Value &&
                a.CreatedAt >= startedAt))
        {
            return;
        }

        try
        {
            await activityLogService.LogAsync(
                ResolveEntityType(context),
                ResolveEntityId(context),
                ResolveAction(context.Request.Method),
                BuildDescription(context),
                null,
                new
                {
                    method = context.Request.Method,
                    path = context.Request.Path.Value,
                    queryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : null,
                    statusCode = context.Response.StatusCode,
                    routeValues = context.Request.RouteValues.ToDictionary(k => k.Key, v => v.Value?.ToString())
                });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to write API activity audit log for {Method} {Path}",
                context.Request.Method,
                context.Request.Path.Value);
        }
    }

    private static bool ShouldAudit(HttpContext context, ICurrentUserService currentUser)
    {
        if (!currentUser.IsAuthenticated)
            return false;

        if (!AuditedMethods.Contains(context.Request.Method))
            return false;

        if (context.Response.StatusCode >= StatusCodes.Status400BadRequest)
            return false;

        var path = context.Request.Path.Value ?? string.Empty;
        return path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase) &&
               !path.StartsWith("/api/v1/activity", StringComparison.OrdinalIgnoreCase);
    }

    private static ActivityType ResolveAction(string method)
    {
        if (HttpMethods.IsPost(method))
            return ActivityType.Create;

        if (HttpMethods.IsDelete(method))
            return ActivityType.Delete;

        return ActivityType.Update;
    }

    private static string ResolveEntityType(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (path.Contains("/ai-content", StringComparison.OrdinalIgnoreCase))
            return "AIContentGeneration";

        var controller = context.Request.RouteValues["controller"]?.ToString();
        return string.IsNullOrWhiteSpace(controller)
            ? "ApiRequest"
            : controller.TrimEnd('s');
    }

    private static Guid? ResolveEntityId(HttpContext context)
    {
        foreach (var key in new[] { "id", "postId", "leadId", "customerId", "propertyId" })
        {
            if (context.Request.RouteValues.TryGetValue(key, out var value) &&
                Guid.TryParse(value?.ToString(), out var id))
            {
                return id;
            }
        }

        return null;
    }

    private static string BuildDescription(HttpContext context)
    {
        var action = context.Request.RouteValues["action"]?.ToString();
        var controller = context.Request.RouteValues["controller"]?.ToString();

        if (!string.IsNullOrWhiteSpace(controller) && !string.IsNullOrWhiteSpace(action))
            return $"{context.Request.Method} {controller}.{action}";

        return $"{context.Request.Method} {context.Request.Path.Value}";
    }
}
