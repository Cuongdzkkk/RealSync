using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RealSync.Core.Entities;
using RealSync.Core.Enums;
using RealSync.Core.Interfaces;
using RealSync.Data.Context;
using RealSync.Shared.DTOs.Requests.ActivityLogs;
using RealSync.Shared.DTOs.Responses.ActivityLogs;
using ValidationException = RealSync.Shared.Exceptions.ValidationException;

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
        await LogForUserAsync(_currentUser.UserId, entityType, entityId, action, description, oldValues, newValues);
    }

    public async Task LogForUserAsync(
        Guid? userId,
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
            UserId = userId,
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

    public async Task<(IReadOnlyList<ActivityLogDto> Items, int TotalCount)> GetActivityLogsAsync(ActivityLogQueryDto query)
    {
        IQueryable<ActivityLog> logs = _context.ActivityLogs
            .Include(a => a.User)
                .ThenInclude(u => u!.Role)
            .AsNoTracking();

        if (query.UserId.HasValue)
            logs = logs.Where(a => a.UserId == query.UserId.Value);

        if (!string.IsNullOrWhiteSpace(query.EntityType))
        {
            var entityType = query.EntityType.Trim();
            logs = logs.Where(a => a.EntityType == entityType);
        }

        if (query.EntityId.HasValue)
            logs = logs.Where(a => a.EntityId == query.EntityId.Value);

        if (!string.IsNullOrWhiteSpace(query.Action))
        {
            if (!Enum.TryParse<ActivityType>(query.Action.Trim(), true, out var action))
                throw new ValidationException("action", "Loại hoạt động không hợp lệ.");

            logs = logs.Where(a => a.Action == action);
        }

        if (query.FromDate.HasValue)
            logs = logs.Where(a => a.CreatedAt >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            logs = logs.Where(a => a.CreatedAt <= query.ToDate.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var keyword = query.Search.Trim();
            logs = logs.Where(a =>
                a.EntityType.Contains(keyword) ||
                (a.Description != null && a.Description.Contains(keyword)) ||
                (a.User != null && (a.User.FullName.Contains(keyword) || a.User.Email.Contains(keyword))));
        }

        var totalCount = await logs.CountAsync();
        logs = ApplySorting(logs, query.SortBy, query.SortDirection);

        var items = await logs
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(a => new ActivityLogDto
            {
                Id = a.Id,
                UserId = a.UserId,
                UserName = a.User != null ? a.User.FullName : null,
                UserEmail = a.User != null ? a.User.Email : null,
                UserRole = a.User != null ? a.User.Role.Name : null,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                Action = a.Action.ToString(),
                Description = a.Description,
                OldValues = a.OldValues,
                NewValues = a.NewValues,
                IpAddress = a.IpAddress,
                UserAgent = a.UserAgent,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        return (items, totalCount);
    }

    private static IQueryable<ActivityLog> ApplySorting(
        IQueryable<ActivityLog> query,
        string? sortBy,
        string? sortDirection)
    {
        var ascending = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);

        return sortBy?.ToLowerInvariant() switch
        {
            "action" => ascending ? query.OrderBy(a => a.Action) : query.OrderByDescending(a => a.Action),
            "entitytype" => ascending ? query.OrderBy(a => a.EntityType) : query.OrderByDescending(a => a.EntityType),
            "username" => ascending ? query.OrderBy(a => a.User!.FullName) : query.OrderByDescending(a => a.User!.FullName),
            "createdat" => ascending ? query.OrderBy(a => a.CreatedAt) : query.OrderByDescending(a => a.CreatedAt),
            _ => query.OrderByDescending(a => a.CreatedAt)
        };
    }
}
