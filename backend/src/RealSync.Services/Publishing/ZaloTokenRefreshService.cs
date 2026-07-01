using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RealSync.Core.Enums;
using RealSync.Core.Interfaces.Publishing;
using RealSync.Data.Context;

namespace RealSync.Services.Publishing;

/// <summary>
/// Service làm mới access token Zalo OA trước khi hết hạn.
/// Zalo access token có hiệu lực ~25 giờ, refresh token hiệu lực 3 tháng.
/// Mỗi lần refresh sẽ nhận được refresh token mới.
/// </summary>
public class ZaloTokenRefreshService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ZaloTokenRefreshService> _logger;

    // Zalo OAuth v4 token endpoint
    private const string TokenEndpoint = "https://oauth.zaloapp.com/v4/access_token";
    // Refresh nếu token hết hạn trong vòng 2 giờ
    private static readonly TimeSpan RefreshThreshold = TimeSpan.FromHours(2);

    public ZaloTokenRefreshService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<ZaloTokenRefreshService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Kiểm tra và refresh tất cả Zalo OA account có token sắp hết hạn.
    /// Được gọi từ Hangfire recurring job.
    /// </summary>
    public virtual async Task RefreshExpiringTokensAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RealSyncDbContext>();
        var vault = scope.ServiceProvider.GetRequiredService<ICredentialVault>();

        var threshold = DateTime.UtcNow.Add(RefreshThreshold);

        var expiringAccounts = await context.ConnectedAccounts
            .Include(a => a.OAuthCredential)
            .Where(a => a.Provider == "Zalo"
                && a.Status == CredentialStatus.Active
                && a.TokenExpiresAt.HasValue
                && a.TokenExpiresAt.Value < threshold)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Zalo token refresh: Tìm thấy {Count} tài khoản cần refresh.", expiringAccounts.Count);

        foreach (var account in expiringAccounts)
        {
            try
            {
                await RefreshSingleAccountAsync(account, vault, context, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi refresh token cho Zalo account {AccountId} ({DisplayName})",
                    account.Id, account.DisplayName);

                account.LastErrorCode = "REFRESH_FAILED";
                account.LastErrorMessage = $"Lỗi refresh token: {ex.Message}";
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Refresh token cho một account cụ thể.
    /// </summary>
    public virtual async Task RefreshSingleAccountAsync(
        Core.Entities.ConnectedAccount account,
        ICredentialVault vault,
        RealSyncDbContext context,
        CancellationToken cancellationToken)
    {
        if (account.OAuthCredential == null
            || string.IsNullOrEmpty(account.OAuthCredential.RefreshTokenEncrypted)
            || string.IsNullOrEmpty(account.OAuthCredential.EncryptionKeyVersion))
        {
            _logger.LogWarning("Zalo account {AccountId} không có refresh token.", account.Id);
            account.Status = CredentialStatus.Expired;
            account.LastErrorCode = "NO_REFRESH_TOKEN";
            account.LastErrorMessage = "Không có refresh token. Vui lòng kết nối lại.";
            return;
        }

        var refreshToken = vault.Decrypt(
            account.OAuthCredential.RefreshTokenEncrypted,
            account.OAuthCredential.EncryptionKeyVersion);

        var appId = !string.IsNullOrEmpty(account.ExternalParentAccountId)
            ? account.ExternalParentAccountId
            : _configuration["Zalo:AppId"] ?? "";

        // Gọi Zalo OAuth v4 refresh
        using var httpClient = new HttpClient();
        var requestContent = new FormUrlEncodedContent(new[]
        {
            new System.Collections.Generic.KeyValuePair<string, string>("refresh_token", refreshToken),
            new System.Collections.Generic.KeyValuePair<string, string>("app_id", appId),
            new System.Collections.Generic.KeyValuePair<string, string>("grant_type", "refresh_token")
        });

        // Zalo OAuth v4 yêu cầu secret_key trong header
        using var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint);
        request.Content = requestContent;
        request.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");

        var appSecret = _configuration["Zalo:AppSecret"];
        if (string.IsNullOrEmpty(appSecret))
        {
            _logger.LogWarning("Zalo:AppSecret cấu hình bị thiếu. Tiến trình refresh token Zalo có thể thất bại.");
        }
        else
        {
            request.Headers.Add("secret_key", appSecret);
        }

        var response = await httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        _logger.LogDebug("Zalo refresh token response for {AccountId}: {StatusCode} {Body}",
            account.Id, response.StatusCode, responseBody);

        using var doc = JsonDocument.Parse(responseBody);
        var root = doc.RootElement;

        // Kiểm tra lỗi
        if (root.TryGetProperty("error", out var errorProp) && errorProp.GetInt32() != 0)
        {
            var errorCode = errorProp.GetInt32();
            var errorMsg = root.TryGetProperty("message", out var msgProp)
                ? msgProp.GetString() ?? "Lỗi refresh token"
                : "Lỗi refresh token";

            _logger.LogWarning("Zalo refresh token failed for {AccountId}: {ErrorCode} {ErrorMsg}",
                account.Id, errorCode, errorMsg);

            account.OAuthCredential.LastRefreshError = $"Zalo error {errorCode}: {errorMsg}";
            account.OAuthCredential.LastRefreshAt = DateTime.UtcNow;

            if (errorCode == -201 || errorCode == -202)
            {
                account.Status = CredentialStatus.Expired;
                account.OAuthCredential.CredentialStatus = CredentialStatus.Expired;
            }
            return;
        }

        // Lấy token mới
        var newAccessToken = root.TryGetProperty("access_token", out var atProp) ? atProp.GetString() : null;
        var newRefreshToken = root.TryGetProperty("refresh_token", out var rtProp) ? rtProp.GetString() : null;
        var expiresIn = root.TryGetProperty("expires_in", out var expProp) ? expProp.GetInt64() : 90000; // default ~25 giờ

        if (string.IsNullOrEmpty(newAccessToken))
        {
            _logger.LogWarning("Zalo refresh response không chứa access_token cho {AccountId}", account.Id);
            account.OAuthCredential.LastRefreshError = "Response không chứa access_token";
            account.OAuthCredential.LastRefreshAt = DateTime.UtcNow;
            return;
        }

        // Encrypt và lưu token mới
        var (accessEnc, keyVersion) = vault.Encrypt(newAccessToken);
        account.OAuthCredential.AccessTokenEncrypted = accessEnc;
        account.OAuthCredential.EncryptionKeyVersion = keyVersion;

        if (!string.IsNullOrEmpty(newRefreshToken))
        {
            var (refreshEnc, _) = vault.Encrypt(newRefreshToken);
            account.OAuthCredential.RefreshTokenEncrypted = refreshEnc;
        }

        var newExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn);
        account.OAuthCredential.ExpiresAt = newExpiresAt;
        account.OAuthCredential.CredentialStatus = CredentialStatus.Active;
        account.OAuthCredential.LastRefreshAt = DateTime.UtcNow;
        account.OAuthCredential.LastRefreshError = null;

        account.TokenExpiresAt = newExpiresAt;
        account.Status = CredentialStatus.Active;
        account.LastValidatedAt = DateTime.UtcNow;
        account.LastErrorCode = null;
        account.LastErrorMessage = null;

        _logger.LogInformation(
            "Zalo OA token refreshed successfully for {AccountId} ({DisplayName}). Expires at {ExpiresAt}.",
            account.Id, account.DisplayName, newExpiresAt);
    }
}
