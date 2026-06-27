using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RealSync.Core.Entities;
using RealSync.Core.Enums;
using RealSync.Core.Interfaces;
using RealSync.Data.Context;
using RealSync.Shared.DTOs.Requests.Auth;
using RealSync.Shared.DTOs.Responses.Auth;
using RealSync.Shared.Exceptions;

namespace RealSync.Services.Implementations;

/// <summary>
/// Service xử lý authentication: login, register, refresh token.
/// </summary>
public class AuthService : IAuthService
{
    private readonly RealSyncDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IActivityLogService _activityLogService;

    public AuthService(
        RealSyncDbContext context,
        IConfiguration configuration,
        IActivityLogService activityLogService)
    {
        _context = context;
        _configuration = configuration;
        _activityLogService = activityLogService;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new BusinessException("Email hoặc mật khẩu không đúng.");

        if (!user.IsActive)
            throw new ForbiddenException("Tài khoản đã bị khóa.");

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await _activityLogService.LogForUserAsync(
            user.Id,
            "Auth",
            user.Id,
            ActivityType.Login,
            "User logged in",
            null,
            new { user.Email, user.FullName, role = user.Role.Name });

        return GenerateAuthResponse(user);
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            throw new BusinessException("Email đã được sử dụng.");

        var defaultRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == "Agent")
            ?? throw new BusinessException("Role mặc định không tồn tại.");

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Phone = request.Phone,
            RoleId = defaultRole.Id,
            IsActive = true,
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Reload với Role
        user.Role = defaultRole;
        await _activityLogService.LogForUserAsync(
            user.Id,
            "User",
            user.Id,
            ActivityType.Create,
            "User registered",
            null,
            new { user.Email, user.FullName, role = defaultRole.Name });

        return GenerateAuthResponse(user);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        // Đơn giản: validate refresh token từ JWT claims
        var principal = GetPrincipalFromExpiredToken(refreshToken);
        var userId = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null || !Guid.TryParse(userId, out var id))
            throw new UnauthorizedException("Refresh token không hợp lệ.");

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new UnauthorizedException("Người dùng không tồn tại.");

        if (!user.IsActive)
            throw new ForbiddenException("Tài khoản đã bị khóa.");

        return GenerateAuthResponse(user);
    }

    private AuthResponse GenerateAuthResponse(User user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(
            _configuration.GetValue<int>("Jwt:ExpiryMinutes", 60));

        return new AuthResponse
        {
            AccessToken = GenerateJwtToken(user, expiresAt),
            RefreshToken = GenerateRefreshToken(user),
            ExpiresAt = expiresAt,
            User = new UserInfo
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                AvatarUrl = user.AvatarUrl,
                Role = user.Role.Name,
            }
        };
    }

    private string GenerateJwtToken(User user, DateTime expiresAt)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.Name),
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken(User user)
    {
        // Refresh token = JWT dài hạn (7 ngày)
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("token_type", "refresh"),
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false, // Cho phép expired token
            ValidateIssuerSigningKey = true,
            ValidIssuer = _configuration["Jwt:Issuer"],
            ValidAudience = _configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!))
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.ValidateToken(token, tokenValidationParameters, out _);
    }
}
