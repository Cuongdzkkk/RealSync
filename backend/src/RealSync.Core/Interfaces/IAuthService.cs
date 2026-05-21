using RealSync.Shared.DTOs.Requests.Auth;
using RealSync.Shared.DTOs.Responses.Auth;

namespace RealSync.Core.Interfaces;

/// <summary>
/// Interface cho Authentication service.
/// </summary>
public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
}
