using Microsoft.AspNetCore.Mvc;

namespace RealSync.Api.Filters;

/// <summary>
/// Attribute yêu cầu permission cụ thể để truy cập endpoint.
/// Ví dụ: [RequirePermission("leads.create")]
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class RequirePermissionAttribute : TypeFilterAttribute
{
    public RequirePermissionAttribute(string permission)
        : base(typeof(PermissionAuthorizationFilter))
    {
        Arguments = new object[] { permission };
    }
}
