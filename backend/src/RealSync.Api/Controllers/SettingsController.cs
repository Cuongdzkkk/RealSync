using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.Json;

namespace RealSync.Api.Controllers;

/// <summary>
/// Controller quản lý cấu hình các kênh đăng bài.
/// Route: /api/v1/settings/channels
/// </summary>
[Authorize]
[Route("api/v1/settings")]
[ApiController]
public class SettingsController : BaseController
{
    private static readonly string ProjectRoot = FindProjectRoot();
    private static readonly string ConfigPath = Path.Combine(ProjectRoot, "channels_config.json");

    /// <summary>
    /// Lấy cấu hình các kênh hiện tại.
    /// </summary>
    [HttpGet("channels")]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<ChannelsConfigDto>), 200)]
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
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<ChannelsConfigDto>), 200)]
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
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<List<FacebookPageDto>>), 200)]
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

            // 1. Exchange short-lived token to long-lived if app id & secret are provided
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

            // 2. Fetch pages list
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

