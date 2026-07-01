using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RealSync.Shared.DTOs.Requests.Publishing;
using RealSync.Shared.DTOs.Responses.Publishing;

namespace RealSync.Core.Interfaces.Publishing;

public interface IConnectedAccountService
{
    Task<IEnumerable<ConnectedAccountResponse>> GetListAsync(CancellationToken cancellationToken);
    Task<ConnectedAccountResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ConnectedAccountResponse> CreateAsync(ConnectedAccountCreateRequest request, CancellationToken cancellationToken);
    Task<ConnectedAccountResponse> ReconnectAsync(Guid id, ConnectedAccountReconnectRequest request, CancellationToken cancellationToken);
    Task<ConnectedAccountResponse> CheckHealthAsync(Guid id, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<ConnectedAccountAuditLogResponse>> GetAuditLogsAsync(Guid id, CancellationToken cancellationToken);
    Task<ConnectedAccountDiagnosticsResponse> GetDiagnosticsAsync(Guid id, CancellationToken cancellationToken);
    Task<ChannelCapabilitiesResponse> GetCapabilitiesAsync(Guid id, CancellationToken cancellationToken);
    Task<TikTokOAuthAuthorizeResponse> GetTikTokAuthorizeUrlAsync(CancellationToken cancellationToken);
    Task<ConnectedAccountResponse> HandleTikTokCallbackAsync(string code, string state, CancellationToken cancellationToken);
    Task<TikTokCreatorInfoResponse> GetTikTokCreatorInfoAsync(Guid id, CancellationToken cancellationToken);
}

