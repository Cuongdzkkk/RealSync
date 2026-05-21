namespace RealSync.Shared.Exceptions;

/// <summary>
/// Exception cho lỗi nghiệp vụ (400).
/// Sử dụng: throw new BusinessException("Sản phẩm đã hết hạn");
/// </summary>
public class BusinessException : AppException
{
    public BusinessException(string message)
        : base(message, 400)
    {
    }
}
