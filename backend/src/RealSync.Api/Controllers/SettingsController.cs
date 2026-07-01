using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RealSync.Services.Options;
using RealSync.Shared.DTOs.Responses;
using RealSync.Shared.Exceptions;

namespace RealSync.Api.Controllers;

/// <summary>
/// Controller quản lý cấu hình các kênh đăng bài và hệ thống.
/// </summary>
[Authorize]
[Route("api/v1/settings")]
[ApiController]
public class SettingsController : BaseController
{
    private static readonly string ProjectRoot = FindProjectRoot();
    private static readonly string ConfigPath = Path.Combine(ProjectRoot, "channels_config.json");
    private static readonly string PlatformConfigPath = Path.Combine(ProjectRoot, "platform_config.json");

    private readonly IOptions<AIOptions> _aiOptions;

    public SettingsController(IOptions<AIOptions> aiOptions)
    {
        _aiOptions = aiOptions;
    }

    /// <summary>
    /// Lấy cấu hình các kênh hiện tại.
    /// </summary>
    [HttpGet("channels")]
    [ProducesResponseType(typeof(ApiResponse<ChannelsConfigDto>), 200)]
    public IActionResult GetChannelsConfig()
    {
        var config = new ChannelsConfigDto();
        if (System.IO.File.Exists(ConfigPath))
        {
            try
            {
                var content = System.IO.File.ReadAllText(ConfigPath);
                var loaded = JsonSerializer.Deserialize<ChannelsConfigDto>(content);
                if (loaded != null)
                {
                    config = loaded;
                }
            }
            catch { /* bypass error */ }
        }
        return OkResponse(config);
    }

    /// <summary>
    /// Lưu cấu hình các kênh mới.
    /// </summary>
    [HttpPost("channels")]
    [ProducesResponseType(typeof(ApiResponse<ChannelsConfigDto>), 200)]
    public IActionResult SaveChannelsConfig([FromBody] ChannelsConfigDto request)
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var content = JsonSerializer.Serialize(request, options);
            System.IO.File.WriteAllText(ConfigPath, content);
            return OkResponse(request, "Đã lưu cấu hình kênh thành công.");
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Lỗi khi lưu cấu hình: {ex.Message}" });
        }
    }

    /// <summary>
    /// Lấy thông tin cấu hình AI hiện tại ở backend.
    /// </summary>
    [HttpGet("ai-info")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public IActionResult GetAiInfo()
    {
        return OkResponse(new
        {
            provider = _aiOptions.Value.Provider,
            model = _aiOptions.Value.Model,
            isConfigured = _aiOptions.Value.IsConfigured
        });
    }

    /// <summary>
    /// Lấy cấu hình Platform Template (Mẫu cấu trúc thương hiệu).
    /// </summary>
    [HttpGet("platform")]
    [ProducesResponseType(typeof(ApiResponse<PlatformConfigDto>), 200)]
    public IActionResult GetPlatformConfig()
    {
        var config = new PlatformConfigDto();
        if (System.IO.File.Exists(PlatformConfigPath))
        {
            try
            {
                var content = System.IO.File.ReadAllText(PlatformConfigPath);
                var loaded = JsonSerializer.Deserialize<PlatformConfigDto>(content);
                if (loaded != null)
                {
                    config = loaded;
                }
            }
            catch { /* bypass */ }
        }
        return OkResponse(config);
    }

    /// <summary>
    /// Lưu cấu hình Platform Template (Mẫu cấu trúc thương hiệu).
    /// </summary>
    [HttpPost("platform")]
    [ProducesResponseType(typeof(ApiResponse<PlatformConfigDto>), 200)]
    public IActionResult SavePlatformConfig([FromBody] PlatformConfigDto request)
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var content = JsonSerializer.Serialize(request, options);
            System.IO.File.WriteAllText(PlatformConfigPath, content);
            return OkResponse(request, "Đã lưu mẫu cấu trúc bài viết thành công.");
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Lỗi khi lưu cấu hình: {ex.Message}" });
        }
    }

    /// <summary>
    /// Trao đổi Authorization Code lấy Access Token và Refresh Token từ Zalo OA.
    /// </summary>
    [HttpPost("zalo/exchange-token")]
    [ProducesResponseType(typeof(ApiResponse<ZaloTokenExchangeResponse>), 200)]
    public async Task<IActionResult> ExchangeZaloToken([FromBody] ZaloTokenExchangeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            throw new BusinessException("Mã phân quyền (Authorization Code) không được để trống.");
        if (string.IsNullOrWhiteSpace(request.AppId))
            throw new BusinessException("Zalo App ID không được để trống.");
        if (string.IsNullOrWhiteSpace(request.AppSecret))
            throw new BusinessException("Zalo App Secret không được để trống.");

        try
        {
            using var client = new HttpClient();
            var tokenUrl = "https://oauth.zalo.me/v2.0/oa/access_token";

            var formContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "code", request.Code },
                { "app_id", request.AppId },
                { "grant_type", "authorization_code" }
            });

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
            httpRequest.Headers.Add("secret_key", request.AppSecret);
            httpRequest.Content = formContent;

            var response = await client.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new BusinessException($"Lỗi kết nối Zalo API ({response.StatusCode}): {responseContent}");
            }

            using var doc = JsonDocument.Parse(responseContent);
            var root = doc.RootElement;

            if (root.TryGetProperty("error", out var errEl))
            {
                var errCode = errEl.GetInt32();
                if (errCode != 0)
                {
                    var errMsg = root.TryGetProperty("error_description", out var descEl) ? descEl.GetString() : "Lỗi không xác định từ Zalo";
                    throw new BusinessException($"Lỗi Zalo OA OAuth (Mã {errCode}): {errMsg}");
                }
            }

            var accessToken = root.GetProperty("access_token").GetString() ?? "";
            var refreshToken = root.TryGetProperty("refresh_token", out var refEl) ? refEl.GetString() : "";
            var expiresInStr = root.TryGetProperty("expires_in", out var expEl) ? expEl.GetString() : "86400";
            int.TryParse(expiresInStr, out var expiresIn);
            if (expiresIn == 0) expiresIn = 86400;

            return OkResponse(new ZaloTokenExchangeResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken ?? "",
                ExpiresInSeconds = expiresIn
            }, "Trao đổi mã token Zalo OA thành công.");
        }
        catch (Exception ex)
        {
            throw new BusinessException($"Không thể lấy token Zalo OA. Chi tiết: {ex.Message}");
        }
    }

    private static string FindProjectRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (System.IO.File.Exists(Path.Combine(dir.FullName, "opencode.json")))
                return dir.FullName;
            dir = dir.Parent;
        }
        return Directory.GetCurrentDirectory();
    }

    /// <summary>
    /// Lấy danh sách trang Facebook được quản lý bởi token người dùng.
    /// </summary>
    [HttpPost("facebook/fetch-pages")]
    [ProducesResponseType(typeof(ApiResponse<List<FacebookPageDto>>), 200)]
    public async Task<IActionResult> FetchFacebookPages([FromBody] FetchFacebookPagesRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserAccessToken))
        {
            return BadRequest(new { message = "Vui lòng nhập User Access Token." });
        }

        try
        {
            using var client = new HttpClient();
            var userToken = request.UserAccessToken;

            if (!string.IsNullOrWhiteSpace(request.AppId) && !string.IsNullOrWhiteSpace(request.AppSecret))
            {
                var exchangeUrl = $"https://graph.facebook.com/v22.0/oauth/access_token?grant_type=fb_exchange_token&client_id={request.AppId}&client_secret={request.AppSecret}&fb_exchange_token={request.UserAccessToken}";
                var exchangeResponse = await client.GetAsync(exchangeUrl);
                var exchangeContent = await exchangeResponse.Content.ReadAsStringAsync();

                if (exchangeResponse.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(exchangeContent);
                    if (doc.RootElement.TryGetProperty("access_token", out var tokenProp))
                    {
                        userToken = tokenProp.GetString() ?? userToken;
                    }
                }
                else
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(exchangeContent);
                        if (doc.RootElement.TryGetProperty("error", out var errorProp) && errorProp.TryGetProperty("message", out var msgProp))
                        {
                            return BadRequest(new { message = $"Lỗi khi trao đổi token người dùng dài hạn: {msgProp.GetString()}" });
                        }
                    }
                    catch {}
                }
            }

            var accountsUrl = $"https://graph.facebook.com/v22.0/me/accounts?access_token={userToken}&limit=100";
            var accountsResponse = await client.GetAsync(accountsUrl);
            var accountsContent = await accountsResponse.Content.ReadAsStringAsync();

            if (!accountsResponse.IsSuccessStatusCode)
            {
                try
                {
                    using var doc = JsonDocument.Parse(accountsContent);
                    if (doc.RootElement.TryGetProperty("error", out var errorProp) && errorProp.TryGetProperty("message", out var msgProp))
                    {
                        return BadRequest(new { message = $"Lỗi từ Facebook: {msgProp.GetString()}" });
                    }
                }
                catch {}
                return BadRequest(new { message = "Không thể lấy danh sách trang từ Facebook." });
            }

            var pages = new List<FacebookPageDto>();
            using (var doc = JsonDocument.Parse(accountsContent))
            {
                if (doc.RootElement.TryGetProperty("data", out var dataProp) && dataProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in dataProp.EnumerateArray())
                    {
                        var page = new FacebookPageDto();
                        if (item.TryGetProperty("id", out var idProp))
                            page.Id = idProp.GetString() ?? string.Empty;
                        if (item.TryGetProperty("name", out var nameProp))
                            page.Name = nameProp.GetString() ?? string.Empty;
                        if (item.TryGetProperty("access_token", out var tokenProp))
                            page.AccessToken = tokenProp.GetString() ?? string.Empty;
                        
                        pages.Add(page);
                    }
                }
            }

            return OkResponse(pages, "Lấy danh sách trang thành công.");
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Lỗi kết nối Facebook: {ex.Message}" });
        }
    }
}

public class ChannelsConfigDto
{
    public string FacebookPageId { get; set; } = string.Empty;
    public string FacebookAccessToken { get; set; } = string.Empty;
    public string MetaAppId { get; set; } = string.Empty;
    public string MetaAppSecret { get; set; } = string.Empty;
    public string ZaloPageId { get; set; } = string.Empty;
    public string ZaloTargetGroupId { get; set; } = string.Empty;
    public string FacebookGroupIds { get; set; } = string.Empty;
}

public class PlatformConfigDto
{
    public bool UsePlatformTemplate { get; set; } = false;
    public string TemplateContent { get; set; } = string.Empty;
}

public class ZaloTokenExchangeRequest
{
    public string Code { get; set; } = string.Empty;
    public string AppId { get; set; } = string.Empty;
    public string AppSecret { get; set; } = string.Empty;
}

public class ZaloTokenExchangeResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresInSeconds { get; set; } = 86400;
}

public class FetchFacebookPagesRequest
{
    public string UserAccessToken { get; set; } = string.Empty;
    public string AppId { get; set; } = string.Empty;
    public string AppSecret { get; set; } = string.Empty;
}

public class FacebookPageDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
}
