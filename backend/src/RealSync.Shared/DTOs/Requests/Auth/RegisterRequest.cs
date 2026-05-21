namespace RealSync.Shared.DTOs.Requests.Auth;

/// <summary>
/// Request đăng ký tài khoản.
/// </summary>
public class RegisterRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Phone { get; set; }
}
