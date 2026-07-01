using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using RealSync.Core.Entities;
using RealSync.Core.Enums;
using RealSync.Core.Interfaces.Publishing;
using RealSync.Shared.DTOs.Requests.Publishing;
using RealSync.Shared.DTOs.Responses.Publishing;
using RealSync.Shared.Exceptions;

namespace RealSync.Services.Publishing;

/// <summary>
/// Partial class chua cac phuong thuc lien quan den TikTok.
/// </summary>
public partial class ConnectedAccountService
{
    public Task<TikTokOAuthAuthorizeResponse> GetTikTokAuthorizeUrlAsync(CancellationToken cancellationToken)
    {
        var (url, state) = _tikTokOAuth.CreateAuthorizeUrl();
        return Task.FromResult(new TikTokOAuthAuthorizeResponse(url, state));
    }

    public async Task<ConnectedAccountResponse> HandleTikTokCallbackAsync(
        string code, string state, CancellationToken cancellationToken)
    {
        if (!_tikTokOAuth.ValidateState(state))
            throw new BusinessException("OAuth state khong hop le hoac da het han. Vui long thu lai.");

        var tokenResult = await _tikTokOAuth.ExchangeCodeAsync(code, cancellationToken);
        if (!tokenResult.IsSuccess || string.IsNullOrEmpty(tokenResult.AccessToken))
            throw new BusinessException(tokenResult.ErrorMessage ?? "Khong the lay access token tu TikTok.");

        var openId = tokenResult.OpenId ?? Guid.NewGuid().ToString("N");
        var scopesJson = string.IsNullOrEmpty(tokenResult.Scope)
            ? null
            : System.Text.Json.JsonSerializer.Serialize(
                tokenResult.Scope.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        var existing = await _context.ConnectedAccounts
            .Include(c => c.OAuthCredential)
            .FirstOrDefaultAsync(c => c.Provider == "TikTok" && c.ExternalAccountId == openId, cancellationToken);

        if (existing != null)
        {
            return await ReconnectAsync(existing.Id, new ConnectedAccountReconnectRequest(
                tokenResult.AccessToken, tokenResult.RefreshToken, (int)tokenResult.ExpiresIn
            ), cancellationToken);
        }

        return await CreateAsync(new ConnectedAccountCreateRequest(
            Provider: "TikTok", ChannelType: PublishingChannelType.TikTok,
            DisplayName: "TikTok (" + openId[..Math.Min(8, openId.Length)] + "...)",
            ExternalAccountId: openId, ExternalParentAccountId: null,
            ProfileUrl: null, AvatarUrl: null,
            AccessToken: tokenResult.AccessToken, RefreshToken: tokenResult.RefreshToken,
            ExpiresInSeconds: (int)tokenResult.ExpiresIn, GrantedScopesJson: scopesJson
        ), cancellationToken);
    }

    public async Task<TikTokCreatorInfoResponse> GetTikTokCreatorInfoAsync(Guid id, CancellationToken cancellationToken)
    {
        var account = await _context.ConnectedAccounts
            .Include(c => c.OAuthCredential)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException("ConnectedAccount", id);

        if (account.Provider != "TikTok")
            throw new BusinessException("Endpoint nay chi danh cho tai khoan TikTok.");

        if (account.OAuthCredential == null)
            throw new BusinessException("Chua co credential TikTok. Vui long hoan thanh OAuth.");

        var token = _vault.Decrypt(
            account.OAuthCredential.AccessTokenEncrypted ?? "",
            account.OAuthCredential.EncryptionKeyVersion ?? "");

        var result = await _tikTokConnector.QueryCreatorInfoAsync(token, cancellationToken);
        if (!result.IsSuccess || result.Data == null)
            throw new BusinessException(result.ErrorMessage ?? "Khong the lay creator info tu TikTok.");

        var info = result.Data;
        account.DisplayName = info.CreatorNickname ?? info.CreatorUsername ?? account.DisplayName;
        account.AvatarUrl = info.CreatorAvatarUrl ?? account.AvatarUrl;
        account.LastValidatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        string? restrictionNote = null;
        if (!_tikTokOptions.IsAppAudited)
        {
            restrictionNote = "App chua qua TikTok audit. Dung Upload/Draft hoac hoan tat audit tai TikTok Developer Portal.";
        }

        return new TikTokCreatorInfoResponse(
            info.CreatorUsername, info.CreatorNickname, info.CreatorAvatarUrl,
            info.PrivacyLevelOptions, info.CommentDisabled, info.DuetDisabled,
            info.StitchDisabled, info.MaxVideoPostDurationSec,
            _tikTokOptions.IsAppAudited, restrictionNote
        );
    }

    private async Task CheckTikTokHealthAsync(ConnectedAccount account, CancellationToken cancellationToken)
    {
        if (account.OAuthCredential == null)
        {
            account.Status = CredentialStatus.PendingSetup;
            account.LastErrorCode = "NO_CREDENTIAL";
            account.LastErrorMessage = "Chua thiet lap credential TikTok.";
            return;
        }

        if (account.TokenExpiresAt.HasValue && account.TokenExpiresAt.Value < DateTime.UtcNow)
        {
            if (string.IsNullOrEmpty(account.OAuthCredential.RefreshTokenEncrypted))
            {
                account.Status = CredentialStatus.Expired;
                account.LastErrorCode = "TOKEN_EXPIRED";
                account.LastErrorMessage = "Ma truy cap OAuth da het han su dung.";
                account.OAuthCredential.CredentialStatus = CredentialStatus.Expired;
                return;
            }

            await _tikTokRefresh.RefreshSingleAccountAsync(account, _vault, _tikTokOAuth, _context, cancellationToken);
        }

        if (account.Status != CredentialStatus.Active)
            return;

        try
        {
            var token = _vault.Decrypt(
                account.OAuthCredential.AccessTokenEncrypted ?? "",
                account.OAuthCredential.EncryptionKeyVersion ?? "");

            var creatorResult = await _tikTokConnector.QueryCreatorInfoAsync(token, cancellationToken);
            if (creatorResult.IsSuccess && creatorResult.Data != null)
            {
                account.DisplayName = creatorResult.Data.CreatorNickname
                    ?? creatorResult.Data.CreatorUsername
                    ?? account.DisplayName;
                account.AvatarUrl = creatorResult.Data.CreatorAvatarUrl ?? account.AvatarUrl;
                account.Status = CredentialStatus.Active;
                account.LastErrorCode = null;
                account.LastErrorMessage = null;
                account.OAuthCredential.CredentialStatus = CredentialStatus.Active;
            }
            else if (creatorResult.ErrorCode == "access_token_invalid")
            {
                await _tikTokRefresh.RefreshSingleAccountAsync(account, _vault, _tikTokOAuth, _context, cancellationToken);
            }
            else
            {
                account.Status = CredentialStatus.Invalid;
                account.LastErrorCode = creatorResult.ErrorCode;
                account.LastErrorMessage = creatorResult.ErrorMessage;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "TikTok health check failed for {AccountId}", account.Id);
            account.LastErrorCode = "HEALTH_CHECK_FAILED";
            account.LastErrorMessage = ex.Message;
        }
    }
}