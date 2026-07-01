using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealSync.Core.Entities;
using RealSync.Core.Enums;
using RealSync.Core.Interfaces;
using RealSync.Core.Interfaces.Publishing;
using RealSync.Data.Context;
using RealSync.Shared.DTOs.Requests.Publishing;
using RealSync.Shared.DTOs.Responses.Publishing;
using RealSync.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RealSync.Services.Options;

namespace RealSync.Services.Publishing;

public partial class ConnectedAccountService : IConnectedAccountService
{
    private readonly RealSyncDbContext _context;
    private readonly ICredentialVault _vault;
    private readonly IActivityLogService _activityLog;
    private readonly ICurrentUserService _currentUser;
    private readonly IConnectorResolver _connectorResolver;
    private readonly ZaloTokenRefreshService _refreshService;
    private readonly TikTokOAuthService _tikTokOAuth;
    private readonly TikTokTokenRefreshService _tikTokRefresh;
    private readonly TikTokConnector _tikTokConnector;
    private readonly TikTokOptions _tikTokOptions;
    private readonly ILogger<ConnectedAccountService> _logger;

    public ConnectedAccountService(
        RealSyncDbContext context,
        ICredentialVault vault,
        IActivityLogService activityLog,
        ICurrentUserService currentUser,
        IConnectorResolver connectorResolver,
        ZaloTokenRefreshService refreshService,
        TikTokOAuthService tikTokOAuth,
        TikTokTokenRefreshService tikTokRefresh,
        TikTokConnector tikTokConnector,
        IOptions<TikTokOptions> tikTokOptions,
        ILogger<ConnectedAccountService> logger)
    {
        _context = context;
        _vault = vault;
        _activityLog = activityLog;
        _currentUser = currentUser;
        _connectorResolver = connectorResolver;
        _refreshService = refreshService;
        _tikTokOAuth = tikTokOAuth;
        _tikTokRefresh = tikTokRefresh;
        _tikTokConnector = tikTokConnector;
        _tikTokOptions = tikTokOptions.Value;
        _logger = logger;
    }

    public async Task<IEnumerable<ConnectedAccountResponse>> GetListAsync(CancellationToken cancellationToken)
    {
        var items = await _context.ConnectedAccounts
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
        return items.Select(MapToResponse);
    }

    public async Task<ConnectedAccountResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await _context.ConnectedAccounts.FindAsync(new object[] { id }, cancellationToken)
            ?? throw new NotFoundException("ConnectedAccount", id);
        return MapToResponse(item);
    }

    public async Task<ConnectedAccountResponse> CreateAsync(ConnectedAccountCreateRequest request, CancellationToken cancellationToken)
    {
        var existing = await _context.ConnectedAccounts
            .FirstOrDefaultAsync(c => c.Provider == request.Provider && c.ExternalAccountId == request.ExternalAccountId, cancellationToken);
        if (existing != null)
        {
            throw new BusinessException("Tai khoan lien ket da ton tai.");
        }

        var expiresAt = request.ExpiresInSeconds.HasValue
            ? DateTime.UtcNow.AddSeconds(request.ExpiresInSeconds.Value)
            : (DateTime?)null;

        var account = new ConnectedAccount
        {
            WorkspaceId = Guid.Empty,
            Provider = request.Provider,
            ChannelType = request.ChannelType,
            DisplayName = request.DisplayName,
            ExternalAccountId = request.ExternalAccountId,
            ExternalParentAccountId = request.ExternalParentAccountId,
            ProfileUrl = request.ProfileUrl,
            AvatarUrl = request.AvatarUrl,
            Status = CredentialStatus.Active,
            GrantedScopesJson = request.GrantedScopesJson,
            TokenExpiresAt = expiresAt,
            LastValidatedAt = DateTime.UtcNow
        };

        _context.ConnectedAccounts.Add(account);
        await _context.SaveChangesAsync(cancellationToken);

        var (accessTokenEnc, keyVersion) = _vault.Encrypt(request.AccessToken);
        string? refreshTokenEnc = null;
        if (!string.IsNullOrEmpty(request.RefreshToken))
        {
            (refreshTokenEnc, _) = _vault.Encrypt(request.RefreshToken);
        }

        var credential = new OAuthCredential
        {
            ConnectedAccountId = account.Id,
            EncryptionKeyVersion = keyVersion,
            AccessTokenEncrypted = accessTokenEnc,
            RefreshTokenEncrypted = refreshTokenEnc,
            ExpiresAt = expiresAt,
            CredentialStatus = CredentialStatus.Active,
            LastRefreshAt = DateTime.UtcNow
        };

        _context.OAuthCredentials.Add(credential);
        await _context.SaveChangesAsync(cancellationToken);

        await _activityLog.LogAsync(
            "ConnectedAccount", account.Id, ActivityType.Create,
            "Kết nối thành công tài khoản " + request.Provider);

        return MapToResponse(account);
    }

    public async Task<ConnectedAccountResponse> ReconnectAsync(Guid id, ConnectedAccountReconnectRequest request, CancellationToken cancellationToken)
    {
        var account = await _context.ConnectedAccounts
            .Include(c => c.OAuthCredential)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException("ConnectedAccount", id);

        var expiresAt = request.ExpiresInSeconds.HasValue
            ? DateTime.UtcNow.AddSeconds(request.ExpiresInSeconds.Value)
            : (DateTime?)null;

        account.Status = CredentialStatus.Active;
        account.TokenExpiresAt = expiresAt;
        account.LastValidatedAt = DateTime.UtcNow;
        account.LastErrorCode = null;
        account.LastErrorMessage = null;

        var (accessTokenEnc, keyVersion) = _vault.Encrypt(request.AccessToken);
        string? refreshTokenEnc = null;
        if (!string.IsNullOrEmpty(request.RefreshToken))
        {
            (refreshTokenEnc, _) = _vault.Encrypt(request.RefreshToken);
        }

        if (account.OAuthCredential == null)
        {
            account.OAuthCredential = new OAuthCredential { ConnectedAccountId = account.Id };
            _context.OAuthCredentials.Add(account.OAuthCredential);
        }

        account.OAuthCredential.EncryptionKeyVersion = keyVersion;
        account.OAuthCredential.AccessTokenEncrypted = accessTokenEnc;
        if (refreshTokenEnc != null)
        {
            account.OAuthCredential.RefreshTokenEncrypted = refreshTokenEnc;
        }
        account.OAuthCredential.ExpiresAt = expiresAt;
        account.OAuthCredential.CredentialStatus = CredentialStatus.Active;
        account.OAuthCredential.LastRefreshAt = DateTime.UtcNow;
        account.OAuthCredential.LastRefreshError = null;

        await _context.SaveChangesAsync(cancellationToken);

        await _activityLog.LogAsync(
            "ConnectedAccount", account.Id, ActivityType.Update,
            "Làm mới kết nối cho tài khoản " + account.Provider);

        return MapToResponse(account);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var account = await _context.ConnectedAccounts
            .Include(c => c.OAuthCredential)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException("ConnectedAccount", id);

        if (account.OAuthCredential != null)
        {
            _context.OAuthCredentials.Remove(account.OAuthCredential);
        }
        _context.ConnectedAccounts.Remove(account);
        await _context.SaveChangesAsync(cancellationToken);

        await _activityLog.LogAsync(
            "ConnectedAccount", id, ActivityType.Delete,
            "Xóa tài khoản liên kết " + account.Provider);
    }

    public async Task<IEnumerable<ConnectedAccountAuditLogResponse>> GetAuditLogsAsync(Guid id, CancellationToken cancellationToken)
    {
        var logs = await _context.ActivityLogs
            .Include(l => l.User)
            .Where(l => l.EntityType == "ConnectedAccount" && l.EntityId == id)
            .OrderByDescending(l => l.CreatedAt)
            .Take(50)
            .Select(l => new ConnectedAccountAuditLogResponse(
                l.Id, l.UserId, l.User != null ? l.User.Email : null, l.Action.ToString(), l.Description, l.IpAddress, l.CreatedAt
            ))
            .ToListAsync(cancellationToken);
        return logs;
    }

    public async Task<ChannelCapabilitiesResponse> GetCapabilitiesAsync(Guid id, CancellationToken cancellationToken)
    {
        var account = await _context.ConnectedAccounts
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException("ConnectedAccount", id);

        var connector = _connectorResolver.Resolve(account.ChannelType);
        var caps = await connector.GetCapabilitiesAsync(account, cancellationToken);
        return new ChannelCapabilitiesResponse(
            caps.SupportsDirectPublish, caps.SupportsDraftUpload, caps.SupportsScheduling,
            caps.SupportsVideo, caps.SupportsImages, caps.RequiresFinalUserConfirmation,
            caps.IsAppAudited, caps.RestrictionReason, caps.GrantedScopes
        );
    }

    private static ConnectedAccountResponse MapToResponse(ConnectedAccount account)
    {
        return new ConnectedAccountResponse(
            account.Id, account.Provider, account.ChannelType, account.DisplayName,
            account.ExternalAccountId, account.ExternalParentAccountId, account.ProfileUrl,
            account.AvatarUrl, account.Status, account.GrantedScopesJson, account.TokenExpiresAt,
            account.LastValidatedAt, account.LastErrorCode, account.LastErrorMessage, account.CreatedAt
        );
    }
}
