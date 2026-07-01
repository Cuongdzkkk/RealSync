using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RealSync.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using RealSync.Core.Entities;
using RealSync.Core.Enums;
using RealSync.Shared.DTOs.Responses.Publishing;

namespace RealSync.Services.Publishing;

/// <summary>
/// Partial class chua health check va diagnostics cho ConnectedAccount.
/// </summary>
public partial class ConnectedAccountService
{
    public async Task<ConnectedAccountResponse> CheckHealthAsync(Guid id, CancellationToken cancellationToken)
    {
        var account = await _context.ConnectedAccounts
            .Include(c => c.OAuthCredential)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException("ConnectedAccount", id);

        if (account.Provider == "Zalo")
        {
            if (account.OAuthCredential == null)
            {
                account.Status = CredentialStatus.PendingSetup;
                account.LastErrorCode = "NO_CREDENTIAL";
                account.LastErrorMessage = "Chua thiet lap thong tin dang nhap Zalo OA.";
            }
            else
            {
                bool needRefresh = false;
                string? token = null;
                try
                {
                    token = _vault.Decrypt(account.OAuthCredential.AccessTokenEncrypted ?? "", account.OAuthCredential.EncryptionKeyVersion ?? "");
                }
                catch
                {
                    needRefresh = true;
                }

                if (token != null)
                {
                    var checkResult = await CheckZaloTokenValidAsync(token, cancellationToken);
                    if (checkResult.IsValid)
                    {
                        account.Status = CredentialStatus.Active;
                        account.LastErrorCode = null;
                        account.LastErrorMessage = null;
                        if (checkResult.Info != null)
                        {
                            account.DisplayName = checkResult.Info.OaName ?? account.DisplayName;
                            account.AvatarUrl = checkResult.Info.AvatarUrl ?? account.AvatarUrl;
                        }
                        account.OAuthCredential.CredentialStatus = CredentialStatus.Active;
                    }
                    else if (checkResult.ErrorCode == -201 || checkResult.ErrorCode == -202)
                    {
                        needRefresh = true;
                    }
                    else
                    {
                        account.Status = CredentialStatus.Invalid;
                        account.LastErrorCode = "ZALO_" + Math.Abs(checkResult.ErrorCode);
                        account.LastErrorMessage = checkResult.ErrorMessage;
                        account.OAuthCredential.CredentialStatus = CredentialStatus.Invalid;
                    }
                }

                if (needRefresh)
                {
                    await _refreshService.RefreshSingleAccountAsync(account, _vault, _context, cancellationToken);

                    if (account.Status == CredentialStatus.Active)
                    {
                        try
                        {
                            var newToken = _vault.Decrypt(account.OAuthCredential.AccessTokenEncrypted ?? "", account.OAuthCredential.EncryptionKeyVersion ?? "");
                            var checkResult = await CheckZaloTokenValidAsync(newToken, cancellationToken);
                            if (checkResult.IsValid)
                            {
                                account.Status = CredentialStatus.Active;
                                account.LastErrorCode = null;
                                account.LastErrorMessage = null;
                                if (checkResult.Info != null)
                                {
                                    account.DisplayName = checkResult.Info.OaName ?? account.DisplayName;
                                    account.AvatarUrl = checkResult.Info.AvatarUrl ?? account.AvatarUrl;
                                }
                                account.OAuthCredential.CredentialStatus = CredentialStatus.Active;
                            }
                            else
                            {
                                account.Status = CredentialStatus.Invalid;
                                account.LastErrorCode = "ZALO_" + Math.Abs(checkResult.ErrorCode);
                                account.LastErrorMessage = checkResult.ErrorMessage;
                                account.OAuthCredential.CredentialStatus = CredentialStatus.Invalid;
                            }
                        }
                        catch
                        {
                            account.Status = CredentialStatus.Invalid;
                            account.LastErrorCode = "DECRYPTION_FAILED";
                            account.LastErrorMessage = "Khong the giai ma access token moi.";
                            account.OAuthCredential.CredentialStatus = CredentialStatus.Invalid;
                        }
                    }
                }
            }
        }
        else if (account.Provider == "TikTok")
        {
            await CheckTikTokHealthAsync(account, cancellationToken);
        }
        else
        {
            if (account.TokenExpiresAt.HasValue && account.TokenExpiresAt.Value < DateTime.UtcNow)
            {
                account.Status = CredentialStatus.Expired;
                account.LastErrorCode = "TOKEN_EXPIRED";
                account.LastErrorMessage = "Ma truy cap OAuth da het han su dung.";
                if (account.OAuthCredential != null)
                    account.OAuthCredential.CredentialStatus = CredentialStatus.Expired;
            }
            else
            {
                account.Status = CredentialStatus.Active;
                account.LastErrorCode = null;
                account.LastErrorMessage = null;
                if (account.OAuthCredential != null)
                    account.OAuthCredential.CredentialStatus = CredentialStatus.Active;
            }
        }

        account.LastValidatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        await _activityLog.LogAsync(
            "ConnectedAccount", account.Id, ActivityType.StatusChange,
            "Kiem tra trang thai lien ket. Ket qua: " + account.Status);

        return MapToResponse(account);
    }

    public async Task<ConnectedAccountDiagnosticsResponse> GetDiagnosticsAsync(Guid id, CancellationToken cancellationToken)
    {
        var account = await _context.ConnectedAccounts
            .Include(c => c.OAuthCredential)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException("ConnectedAccount", id);

        string tokenStatus;
        TimeSpan? remainingTtl = null;

        if (account.OAuthCredential == null || string.IsNullOrEmpty(account.OAuthCredential.AccessTokenEncrypted))
        {
            tokenStatus = "NoCredential";
        }
        else if (account.TokenExpiresAt.HasValue)
        {
            var ttl = account.TokenExpiresAt.Value - DateTime.UtcNow;
            if (ttl <= TimeSpan.Zero)
                tokenStatus = "Expired";
            else if (ttl < TimeSpan.FromHours(2))
            {
                tokenStatus = "Expiring";
                remainingTtl = ttl;
            }
            else
            {
                tokenStatus = "Valid";
                remainingTtl = ttl;
            }
        }
        else
        {
            tokenStatus = account.Status == CredentialStatus.Active ? "Valid" : account.Status.ToString();
        }

        string? diagnosticNotes = null;
        string? description = null;
        string? displayName = account.DisplayName;

        if (account.Provider == "Zalo")
        {
            if (tokenStatus == "Expired")
                diagnosticNotes = "Token Zalo OA da het han. Can ket noi lai hoac cho token refresh tu dong.";
            else if (tokenStatus == "Expiring")
                diagnosticNotes = "Token Zalo OA sap het han. He thong se tu dong refresh truoc khi het han.";
            else if (tokenStatus == "NoCredential")
                diagnosticNotes = "Chua co credential. Vui long hoan thanh OAuth flow cho Zalo OA.";

            if (tokenStatus == "Valid" && account.OAuthCredential != null)
            {
                try
                {
                    var token = _vault.Decrypt(account.OAuthCredential.AccessTokenEncrypted ?? "", account.OAuthCredential.EncryptionKeyVersion ?? "");
                    var checkResult = await CheckZaloTokenValidAsync(token, cancellationToken);
                    if (checkResult.IsValid && checkResult.Info != null)
                    {
                        displayName = checkResult.Info.OaName ?? displayName;
                        description = checkResult.Info.Description;
                    }
                }
                catch { }
            }
        }
        else if (account.Provider == "TikTok")
        {
            if (!_tikTokOptions.IsAppAudited)
                diagnosticNotes = "App TikTok chua qua Content Posting API audit. Direct post cong khai khong kha dung.";
            else if (tokenStatus == "Expired")
                diagnosticNotes = "Token TikTok da het han. Vui long ket noi lai hoac cho refresh tu dong.";
            else if (tokenStatus == "NoCredential")
                diagnosticNotes = "Chua co credential TikTok. Vui long hoan thanh OAuth.";
        }

        return new ConnectedAccountDiagnosticsResponse(
            AccountId: account.Id, Provider: account.Provider, Status: account.Status,
            TokenStatus: tokenStatus, RemainingTtl: remainingTtl,
            TokenExpiresAt: account.TokenExpiresAt,
            LastRefreshAt: account.OAuthCredential?.LastRefreshAt,
            LastRefreshError: account.OAuthCredential?.LastRefreshError,
            LastValidatedAt: account.LastValidatedAt, OaName: displayName,
            OaDescription: description, FollowerCount: null,
            GrantedScopes: account.GrantedScopesJson,
            DiagnosticNotes: diagnosticNotes
        );
    }
}