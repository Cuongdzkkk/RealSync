using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using RealSync.Data.Context;
using RealSync.Shared.DTOs.Responses;

namespace RealSync.Api.Filters;

/// <summary>
/// Filter kiểm tra permission từ database.
/// Được gọi thông qua RequirePermissionAttribute.
/// </summary>
public class PermissionAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly string _permission;
    private readonly RealSyncDbContext _context;

    public PermissionAuthorizationFilter(string permission, RealSyncDbContext context)
    {
        _permission = permission;
        _context = context;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedObjectResult(
                ApiResponse<object>.Fail("Chưa đăng nhập.", 401));
            return;
        }

        var roleName = user.FindFirst(ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(roleName))
        {
            context.Result = new ObjectResult(
                ApiResponse<object>.Fail("Không có quyền truy cập.", 403))
            { StatusCode = 403 };
            return;
        }

        // Kiểm tra permission từ database
        var hasPermission = await _context.RolePermissions
            .AnyAsync(rp =>
                rp.Role.Name == roleName &&
                rp.Permission.Name == _permission);

        if (!hasPermission)
        {
            context.Result = new ObjectResult(
                ApiResponse<object>.Fail($"Không có quyền '{_permission}'.", 403))
            { StatusCode = 403 };
        }
    }
}
