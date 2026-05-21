namespace RealSync.Shared.Exceptions;

/// <summary>
/// Exception khi chưa đăng nhập (401).
/// </summary>
public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Bạn cần đăng nhập để thực hiện hành động này.")
        : base(message, 401)
    {
    }
}
