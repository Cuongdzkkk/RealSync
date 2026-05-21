namespace RealSync.Shared.Exceptions;

/// <summary>
/// Exception khi không có quyền truy cập (403).
/// Sử dụng: throw new ForbiddenException("Không có quyền truy cập");
/// </summary>
public class ForbiddenException : AppException
{
    public ForbiddenException(string message = "Bạn không có quyền thực hiện hành động này.")
        : base(message, 403)
    {
    }
}
