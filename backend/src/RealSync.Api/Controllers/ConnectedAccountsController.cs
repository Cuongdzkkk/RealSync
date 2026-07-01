using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RealSync.Api.Filters;
using RealSync.Core.Interfaces.Publishing;
using RealSync.Shared.DTOs.Requests.Publishing;
using RealSync.Shared.DTOs.Responses;
using RealSync.Shared.DTOs.Responses.Publishing;

namespace RealSync.Api.Controllers;

/// <summary>
/// Controller quản lý các tài khoản liên kết đăng bài (Connected Accounts).
/// </summary>
[Authorize]
public class ConnectedAccountsController : BaseController
{
    private readonly IConnectedAccountService _accountService;
    private readonly IConfiguration _configuration;

    public ConnectedAccountsController(
        IConnectedAccountService accountService,
        IConfiguration configuration)
    {
        _accountService = accountService;
        _configuration = configuration;
    }

    [HttpGet]
    [RequirePermission("posts.read")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ConnectedAccountResponse>>), 200)]
    public async Task<IActionResult> GetList(CancellationToken cancellationToken)
    {
        var result = await _accountService.GetListAsync(cancellationToken);
        return OkResponse(result);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("posts.read")]
    [ProducesResponseType(typeof(ApiResponse<ConnectedAccountResponse>), 200)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _accountService.GetByIdAsync(id, cancellationToken);
        return OkResponse(result);
    }

    [HttpPost]
    [RequirePermission("posts.update")]
    [ProducesResponseType(typeof(ApiResponse<ConnectedAccountResponse>), 201)]
    public async Task<IActionResult> Create([FromBody] ConnectedAccountCreateRequest request, CancellationToken cancellationToken)
    {
        var result = await _accountService.CreateAsync(request, cancellationToken);
        return CreatedResponse(result, "Liên kết tài khoản mạng xã hội thành công.");
    }

    [HttpPost("{id:guid}/reconnect")]
    [RequirePermission("posts.update")]
    [ProducesResponseType(typeof(ApiResponse<ConnectedAccountResponse>), 200)]
    public async Task<IActionResult> Reconnect(Guid id, [FromBody] ConnectedAccountReconnectRequest request, CancellationToken cancellationToken)
    {
        var result = await _accountService.ReconnectAsync(id, request, cancellationToken);
        return OkResponse(result, "Đã cập nhật liên kết / làm mới access token.");
    }

    [HttpPost("{id:guid}/check-health")]
    [RequirePermission("posts.update")]
    [ProducesResponseType(typeof(ApiResponse<ConnectedAccountResponse>), 200)]
    public async Task<IActionResult> CheckHealth(Guid id, CancellationToken cancellationToken)
    {
        var result = await _accountService.CheckHealthAsync(id, cancellationToken);
        return OkResponse(result, "Đã hoàn thành kiểm tra trạng thái liên kết.");
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("posts.delete")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _accountService.DeleteAsync(id, cancellationToken);
        return OkResponse<object?>(null, "Đã xóa tài khoản liên kết thành công.");
    }

    [HttpGet("{id:guid}/audit-logs")]
    [RequirePermission("posts.read")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ConnectedAccountAuditLogResponse>>), 200)]
    public async Task<IActionResult> GetAuditLogs(Guid id, CancellationToken cancellationToken)
    {
        var result = await _accountService.GetAuditLogsAsync(id, cancellationToken);
        return OkResponse(result);
    }

    [HttpGet("{id:guid}/diagnostics")]
    [RequirePermission("posts.read")]
    [ProducesResponseType(typeof(ApiResponse<ConnectedAccountDiagnosticsResponse>), 200)]
    public async Task<IActionResult> GetDiagnostics(Guid id, CancellationToken cancellationToken)
    {
        var result = await _accountService.GetDiagnosticsAsync(id, cancellationToken);
        return OkResponse(result, "Thông tin chẩn đoán tài khoản liên kết.");
    }

    [HttpGet("{id:guid}/capabilities")]
    [RequirePermission("posts.read")]
    [ProducesResponseType(typeof(ApiResponse<ChannelCapabilitiesResponse>), 200)]
    public async Task<IActionResult> GetCapabilities(Guid id, CancellationToken cancellationToken)
    {
        var result = await _accountService.GetCapabilitiesAsync(id, cancellationToken);
        return OkResponse(result);
    }

    [HttpGet("tiktok/authorize")]
    [RequirePermission("posts.update")]
    [ProducesResponseType(typeof(ApiResponse<TikTokOAuthAuthorizeResponse>), 200)]
    public async Task<IActionResult> GetTikTokAuthorizeUrl(CancellationToken cancellationToken)
    {
        var result = await _accountService.GetTikTokAuthorizeUrlAsync(cancellationToken);
        return OkResponse(result, "URL OAuth TikTok.");
    }

    [HttpGet("tiktok/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> TikTokOAuthCallback(
        [FromQuery] string code,
        [FromQuery] string state,
        CancellationToken cancellationToken)
    {
        await _accountService.HandleTikTokCallbackAsync(code, state, cancellationToken);
        var frontendUrl = _configuration["Publishing:FrontendSettingsUrl"]
            ?? "http://localhost:5173/admin/settings?tab=connected-accounts&tiktok=connected";
        return Redirect(frontendUrl);
    }

    [HttpGet("{id:guid}/tiktok/creator-info")]
    [RequirePermission("posts.read")]
    [ProducesResponseType(typeof(ApiResponse<TikTokCreatorInfoResponse>), 200)]
    public async Task<IActionResult> GetTikTokCreatorInfo(Guid id, CancellationToken cancellationToken)
    {
        var result = await _accountService.GetTikTokCreatorInfoAsync(id, cancellationToken);
        return OkResponse(result, "Thông tin creator TikTok.");
    }
}

