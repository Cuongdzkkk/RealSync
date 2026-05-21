namespace RealSync.Shared.Exceptions;

/// <summary>
/// Exception cho lỗi validation (422).
/// </summary>
public class ValidationException : AppException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("Dữ liệu không hợp lệ.", 422)
    {
        Errors = errors;
    }

    public ValidationException(string field, string message)
        : base("Dữ liệu không hợp lệ.", 422)
    {
        Errors = new Dictionary<string, string[]>
        {
            { field, new[] { message } }
        };
    }
}
