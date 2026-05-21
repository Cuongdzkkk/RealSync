namespace RealSync.Shared.Exceptions;

/// <summary>
/// Exception khi không tìm thấy resource (404).
/// Sử dụng: throw new NotFoundException("Property", propertyId);
/// </summary>
public class NotFoundException : AppException
{
    public NotFoundException(string entityName, object id)
        : base($"{entityName} với ID '{id}' không tồn tại.", 404)
    {
    }

    public NotFoundException(string message)
        : base(message, 404)
    {
    }
}
