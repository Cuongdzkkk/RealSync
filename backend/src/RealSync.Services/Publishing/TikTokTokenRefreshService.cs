using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RealSync.Core.Enums;
using RealSync.Core.Interfaces.Publishing;
using RealSync.Data.Context;

namespace RealSync.Services.Publishing;

/// <summary>
/// Làm mới access token TikTok trước khi hết hạn.
/// </summary>
public class TikTokTokenRefreshService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TikTokTokenRefreshService> _logger;
    private static readonly TimeSpan RefreshThreshold = TimeSpan.FromHours(2);

    public TikTokTokenRefreshService(
        IServiceScopeFactory scopeFactory,
        ILogger<TikTokTokenRefreshService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public virtual async Task RefreshExpiringTokensAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RealSyncDbContext>();
        var vault = scope.ServiceProvider.GetRequiredService<ICredentialVault>();
        var oauth = scope.ServiceProvider.GetRequiredService<TikTokOAuthService>();

        var threshold = DateTime.UtcNow.Add(RefreshThreshold);

        var expiringAccounts = await context.ConnectedAccounts
            .Include(a => a.OAuthCredential)
            .Where(a => a.Provider == "TikTok"
                && a.Status == CredentialStatus.Active
                && a.TokenExpiresAt.HasValue
                && a.TokenExpiresAt.Value < threshold)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("TikTok token refresh: {Count} tài khoản cần refresh.", expiringAccounts.Count);

        foreach (var account in expiringAccounts)
        {
            try
            {
                await RefreshSingleAccountAsync(account, vault, oauth, context, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi refresh token TikTok account {AccountId}", account.Id);
                account.LastErrorCode = "REFRESH_FAILED";
                account.LastErrorMessage = $"Lỗi refresh token: {ex.Message}";
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task RefreshSingleAccountAsync(
        Core.Entities.ConnectedAccount account,
        ICredentialVault vault,
        TikTokOAuthService oauth,
        RealSyncDbContext context,
        CancellationToken cancellationToken)
    {
        if (account.OAuthCredential == null
            || string.IsNullOrEmpty(account.OAuthCredential.RefreshTokenEncrypted)
            || string.IsNullOrEmpty(account.OAuthCredential.EncryptionKeyVersion))
        {
            account.Status = CredentialStatus.Expired;
            account.LastErrorCode = "NO_REFRESH_TOKEN";
            account.LastErrorMessage = "Không có refresh token. Vui lòng kết nối lại TikTok.";
            return;
        }

        var refreshToken = vault.Decrypt(
            account.OAuthCredential.RefreshTokenEncrypted,
            account.OAuthCredential.EncryptionKeyVersion);

        var result = await oauth.RefreshTokenAsync(refreshToken, cancellationToken);
        if (!result.IsSuccess || string.IsNullOrEmpty(result.AccessToken))
        {
            account.OAuthCredential.LastRefreshError = result.ErrorMessage;
            account.OAuthCredential.LastRefreshAt = DateTime.UtcNow;
            account.Status = CredentialStatus.Expired;
            account.OAuthCredential.CredentialStatus = CredentialStatus.Expired;
            return;
        }

        var (accessEnc, keyVersion) = vault.Encrypt(result.AccessToken);
        account.OAuthCredential.AccessTokenEncrypted = accessEnc;
        account.OAuthCredential.EncryptionKeyVersion = keyVersion;

        if (!string.IsNullOrEmpty(result.RefreshToken))
        {
            var (refreshEnc, _) = vault.Encrypt(result.RefreshToken);
            account.OAuthCredential.RefreshTokenEncrypted = refreshEnc;
        }

        var expiresAt = DateTime.UtcNow.AddSeconds(result.ExpiresIn);
        account.OAuthCredential.ExpiresAt = expiresAt;
        account.OAuthCredential.CredentialStatus = CredentialStatus.Active;
        account.OAuthCredential.LastRefreshAt = DateTime.UtcNow;
        account.OAuthCredential.LastRefreshError = null;
        account.TokenExpiresAt = expiresAt;
        account.Status = CredentialStatus.Active;
        account.LastValidatedAt = DateTime.UtcNow;
        account.LastErrorCode = null;
        account.LastErrorMessage = null;

        if (!string.IsNullOrEmpty(result.Scope))
            account.GrantedScopesJson = JsonQuoteScopes(result.Scope);

        _logger.LogInformation("TikTok token refreshed for {AccountId}. Expires {ExpiresAt}", account.Id, expiresAt);
    }

    private static string JsonQuoteScopes(string scope)
        => System.Text.Json.JsonSerializer.Serialize(
            scope.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
}
