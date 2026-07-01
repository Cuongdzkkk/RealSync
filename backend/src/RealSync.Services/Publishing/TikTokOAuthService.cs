using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RealSync.Core.Interfaces.Publishing;
using RealSync.Services.Options;

namespace RealSync.Services.Publishing;

/// <summary>
/// Xử lý OAuth flow TikTok Login Kit v2.
/// </summary>
public class TikTokOAuthService
{
    private readonly HttpClient _httpClient;
    private readonly TikTokOptions _options;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TikTokOAuthService> _logger;

    private const string TokenEndpoint = "v2/oauth/token/";
    private static readonly TimeSpan StateTtl = TimeSpan.FromMinutes(10);

    public TikTokOAuthService(
        HttpClient httpClient,
        IOptions<TikTokOptions> options,
        IMemoryCache cache,
        ILogger<TikTokOAuthService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _cache = cache;
        _logger = logger;
    }

    public bool IsConfigured =>
        !string.IsNullOrEmpty(_options.ClientKey)
        && !string.IsNullOrEmpty(_options.ClientSecret)
        && !string.IsNullOrEmpty(_options.RedirectUri);

    /// <summary>
    /// Tạo URL authorize và lưu CSRF state vào cache.
    /// </summary>
    public (string AuthorizeUrl, string State) CreateAuthorizeUrl(string? scopes = null)
    {
        if (!IsConfigured)
            throw new InvalidOperationException("TikTok OAuth chưa được cấu hình (ClientKey/ClientSecret/RedirectUri).");

        var state = Guid.NewGuid().ToString("N");
        _cache.Set($"tiktok_oauth_state_{state}", true, StateTtl);

        var scope = scopes ?? _options.DefaultScopes;
        var url = $"{_options.AuthorizeUrl}" +
                  $"?client_key={Uri.EscapeDataString(_options.ClientKey)}" +
                  $"&response_type=code" +
                  $"&scope={Uri.EscapeDataString(scope)}" +
                  $"&redirect_uri={Uri.EscapeDataString(_options.RedirectUri)}" +
                  $"&state={Uri.EscapeDataString(state)}";

        return (url, state);
    }

    public bool ValidateState(string state)
    {
        var key = $"tiktok_oauth_state_{state}";
        if (_cache.TryGetValue(key, out _))
        {
            _cache.Remove(key);
            return true;
        }
        return false;
    }

    public async Task<TikTokTokenResult> ExchangeCodeAsync(string code, CancellationToken cancellationToken)
    {
        return await RequestTokenAsync(new Dictionary<string, string>
        {
            ["client_key"] = _options.ClientKey,
            ["client_secret"] = _options.ClientSecret,
            ["code"] = code,
            ["grant_type"] = "authorization_code",
            ["redirect_uri"] = _options.RedirectUri
        }, cancellationToken);
    }

    public async Task<TikTokTokenResult> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        return await RequestTokenAsync(new Dictionary<string, string>
        {
            ["client_key"] = _options.ClientKey,
            ["client_secret"] = _options.ClientSecret,
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken
        }, cancellationToken);
    }

    private async Task<TikTokTokenResult> RequestTokenAsync(
        Dictionary<string, string> formData, CancellationToken cancellationToken)
    {
        try
        {
            using var content = new FormUrlEncodedContent(formData);
            using var response = await _httpClient.PostAsync(TokenEndpoint, content, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogDebug("TikTok token response: {StatusCode}", response.StatusCode);

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            if (root.TryGetProperty("error", out var errorProp) && errorProp.ValueKind == JsonValueKind.String)
            {
                var errDesc = root.TryGetProperty("error_description", out var descProp)
                    ? descProp.GetString()
                    : errorProp.GetString();
                return TikTokTokenResult.Failure(errDesc ?? "OAuth token error");
            }

            if (!response.IsSuccessStatusCode)
                return TikTokTokenResult.Failure($"HTTP {(int)response.StatusCode}: {body}");

            return new TikTokTokenResult
            {
                IsSuccess = true,
                AccessToken = root.TryGetProperty("access_token", out var at) ? at.GetString() : null,
                RefreshToken = root.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null,
                OpenId = root.TryGetProperty("open_id", out var oid) ? oid.GetString() : null,
                Scope = root.TryGetProperty("scope", out var sc) ? sc.GetString() : null,
                ExpiresIn = root.TryGetProperty("expires_in", out var exp) ? exp.GetInt64() : 86400,
                RefreshExpiresIn = root.TryGetProperty("refresh_expires_in", out var rexp) ? rexp.GetInt64() : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TikTok OAuth token request failed");
            return TikTokTokenResult.Failure($"Lỗi OAuth TikTok: {ex.Message}");
        }
    }
}

public class TikTokTokenResult
{
    public bool IsSuccess { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public string? OpenId { get; set; }
    public string? Scope { get; set; }
    public long ExpiresIn { get; set; }
    public long? RefreshExpiresIn { get; set; }
    public string? ErrorMessage { get; set; }

    public static TikTokTokenResult Failure(string message) => new() { IsSuccess = false, ErrorMessage = message };
}
