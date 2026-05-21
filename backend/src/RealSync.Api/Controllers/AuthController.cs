using Microsoft.AspNetCore.Mvc;
using RealSync.Core.Interfaces;
using RealSync.Shared.DTOs.Requests.Auth;
using RealSync.Shared.DTOs.Responses.Auth;

namespace RealSync.Api.Controllers;

/// <summary>
/// Controller xử lý authentication.
/// </summary>
public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Đăng nhập.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<AuthResponse>), 200)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return OkResponse(result, "Đăng nhập thành công");
    }

    /// <summary>
    /// Đăng ký tài khoản mới.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<AuthResponse>), 201)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        return CreatedResponse(result, "Đăng ký thành công");
    }

    /// <summary>
    /// Refresh access token.
    /// </summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<AuthResponse>), 200)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);
        return OkResponse(result, "Token đã được làm mới");
    }
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
