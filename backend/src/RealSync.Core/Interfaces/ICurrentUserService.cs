namespace RealSync.Core.Interfaces;

/// <summary>
/// Interface lấy thông tin user hiện tại từ JWT claims.
/// Dùng để auto-fill audit fields (CreatedBy, UpdatedBy).
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    string? FullName { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
}
