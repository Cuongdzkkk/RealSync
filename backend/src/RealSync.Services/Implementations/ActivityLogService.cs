using System.Text.Json;
using Microsoft.AspNetCore.Http;
using RealSync.Core.Entities;
using RealSync.Core.Enums;
using RealSync.Core.Interfaces;
using RealSync.Data.Context;

namespace RealSync.Services.Implementations;

/// <summary>
/// Ghi log hoạt động user vào database.
/// Auto-fill userId, IP, UserAgent từ HttpContext.
/// </summary>
public class ActivityLogService : IActivityLogService
{
    private readonly RealSyncDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ActivityLogService(
        RealSyncDbContext context,
        ICurrentUserService currentUser,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _currentUser = currentUser;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(
        string entityType,
        Guid? entityId,
        ActivityType action,
        string? description = null,
        object? oldValues = null,
        object? newValues = null)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        var log = new ActivityLog
        {
            UserId = _currentUser.UserId,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            Description = description,
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
            IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext?.Request.Headers.UserAgent.ToString(),
        };

        _context.ActivityLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}
