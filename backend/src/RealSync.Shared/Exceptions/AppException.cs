namespace RealSync.Shared.Exceptions;

/// <summary>
/// Base exception cho toàn bộ ứng dụng RealSync.
/// Tất cả custom exceptions phải kế thừa từ class này.
/// </summary>
public class AppException : Exception
{
    public int StatusCode { get; }

    public AppException(string message, int statusCode = 400)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
