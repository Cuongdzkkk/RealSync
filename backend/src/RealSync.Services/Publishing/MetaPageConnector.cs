using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RealSync.Core.Entities;
using RealSync.Core.Enums;
using RealSync.Core.Interfaces.Publishing;
using RealSync.Core.Models.Publishing;

namespace RealSync.Services.Publishing;

/// <summary>
/// Connector Facebook Page theo Meta Graph API; khong ho tro Facebook Group direct posting.
/// </summary>
public class MetaPageConnector : IPublishingConnector
{
    private readonly HttpClient _httpClient;
    private readonly ICredentialVault _vault;
    private readonly ILogger<MetaPageConnector> _logger;

    public MetaPageConnector(HttpClient httpClient, ICredentialVault vault, ILogger<MetaPageConnector> logger)
    {
        _httpClient = httpClient;
        _vault = vault;
        _logger = logger;
    }

    public PublishingChannelType ChannelType => PublishingChannelType.FacebookPage;

    public Task<ChannelCapabilities> GetCapabilitiesAsync(ConnectedAccount? account, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ChannelCapabilities
        {
            SupportsDirectPublish = account?.Status == CredentialStatus.Active,
            SupportsDraftUpload = false,
            SupportsScheduling = true,
            SupportsVideo = true,
            SupportsImages = true,
            SupportsUpdate = false,
            SupportsDelete = false,
            RequiresFinalUserConfirmation = false,
            GrantedScopes = PublishingJson.ParseScopes(account?.GrantedScopesJson),
            RestrictionReason = account?.Status == CredentialStatus.Active
                ? null
                : "Chua ket noi Facebook Page access token hop le."
        });
    }

    public Task<ValidationResult> ValidateAsync(PublicationContext context, CancellationToken cancellationToken)
    {
        var accountValidation = PublishingValidation.ActiveOAuthAccount(context.Account, "Facebook Page");
        if (!accountValidation.IsValid) return Task.FromResult(accountValidation);

        var manifest = PublishingJson.Parse<MetaMediaManifest>(context.Job.MediaManifestJson) ?? new MetaMediaManifest();
        if (string.IsNullOrWhiteSpace(context.Variant.Caption)
            && string.IsNullOrWhiteSpace(manifest.LinkUrl)
            && string.IsNullOrWhiteSpace(manifest.ImageUrl)
            && string.IsNullOrWhiteSpace(manifest.VideoUrl))
        {
            return Task.FromResult(ValidationResult.Failure("Facebook Page can caption, link, image hoac video."));
        }

        if (context.Job.PublishMode != PublishMode.Direct)
            return Task.FromResult(ValidationResult.Failure("Facebook Page connector chi ho tro Direct qua Graph API."));

        return Task.FromResult(ValidationResult.Success());
    }

    public async Task<PublishInitiationResult> PublishAsync(PublicationContext context, CancellationToken cancellationToken)
    {
        string accessToken;
        try
        {
            accessToken = PublishingValidation.DecryptAccessToken(context.Account!, _vault);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Khong the giai ma Meta token cho account {AccountId}", context.Account!.Id);
            return PublishInitiationResult.Failure(
                "Khong the giai ma Meta access token. Vui long ket noi lai.",
                "DECRYPTION_FAILED", "AuthenticationError", isRetryable: false);
        }

        var pageId = context.Account!.ExternalAccountId;
        var manifest = PublishingJson.Parse<MetaMediaManifest>(context.Job.MediaManifestJson) ?? new MetaMediaManifest();
        var caption = manifest.Caption ?? context.Variant.Caption ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(manifest.VideoUrl))
        {
            var videoResult = await PostFormAsync(
                $"{pageId}/videos",
                new Dictionary<string, string?>
                {
                    ["file_url"] = manifest.VideoUrl,
                    ["description"] = caption,
                    ["access_token"] = accessToken
                },
                cancellationToken);

            if (!videoResult.IsSuccess) return videoResult;
            context.Job.RemoteStatus = "VIDEO_PROCESSING";
            return PublishInitiationResult.RemoteProcessing(videoResult.ExternalPostId!, context.Job.RemoteStatus);
        }

        if (!string.IsNullOrWhiteSpace(manifest.ImageUrl))
        {
            return await PostFormAsync(
                $"{pageId}/photos",
                new Dictionary<string, string?>
                {
                    ["url"] = manifest.ImageUrl,
                    ["caption"] = caption,
                    ["published"] = "true",
                    ["access_token"] = accessToken
                },
                cancellationToken);
        }

        return await PostFormAsync(
            $"{pageId}/feed",
            new Dictionary<string, string?>
            {
                ["message"] = caption,
                ["link"] = manifest.LinkUrl ?? context.Variant.LinkUrl,
                ["published"] = "true",
                ["access_token"] = accessToken
            },
            cancellationToken);
    }

    public async Task<RemotePublicationStatus> GetStatusAsync(PublicationTrackingContext context, CancellationToken cancellationToken)
    {
        if (context.Account == null || string.IsNullOrWhiteSpace(context.Job.ExternalPublishId))
        {
            return new RemotePublicationStatus
            {
                Status = PublicationJobStatus.NeedsReview,
                ErrorMessage = "Thieu Facebook Page account hoac video id."
            };
        }

        var token = PublishingValidation.DecryptAccessToken(context.Account, _vault);
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{context.Job.ExternalPublishId}?fields=status,permalink_url&access_token={Uri.EscapeDataString(token)}");
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new RemotePublicationStatus
            {
                Status = PublicationJobStatus.NeedsReview,
                ErrorMessage = $"Meta status API loi HTTP {(int)response.StatusCode}."
            };
        }

        var status = PublishingJson.ExtractString(body, "status");
        if (status?.Contains("ready", StringComparison.OrdinalIgnoreCase) == true
            || status?.Contains("published", StringComparison.OrdinalIgnoreCase) == true)
        {
            return new RemotePublicationStatus
            {
                Status = PublicationJobStatus.Published,
                PublishedUrl = PublishingJson.ExtractString(body, "permalink_url"),
                RemoteStatusDescription = "Facebook Page video da san sang."
            };
        }

        return new RemotePublicationStatus
        {
            Status = PublicationJobStatus.RemoteProcessing,
            RemoteStatusDescription = $"Meta video status: {status ?? "unknown"}"
        };
    }

    public Task<DeleteRemoteResult> DeleteAsync(PublicationTrackingContext context, CancellationToken cancellationToken)
    {
        return Task.FromResult(DeleteRemoteResult.Failure(
            "Meta delete chua duoc bat trong Phase 8. Vui long xoa tren Meta Business Suite neu can."));
    }

    private async Task<PublishInitiationResult> PostFormAsync(
        string path,
        Dictionary<string, string?> values,
        CancellationToken cancellationToken)
    {
        var filtered = new Dictionary<string, string>();
        foreach (var item in values)
        {
            if (!string.IsNullOrWhiteSpace(item.Value))
                filtered[item.Key] = item.Value!;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, path);
        request.Content = new FormUrlEncodedContent(filtered);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            return MapGraphFailure(response.StatusCode, body);

        var id = PublishingJson.ExtractString(body, "post_id")
            ?? PublishingJson.ExtractString(body, "id");
        if (string.IsNullOrWhiteSpace(id))
        {
            return PublishInitiationResult.Failure(
                "Meta Graph API khong tra ve id bai dang.",
                "MISSING_META_ID", "ProviderContractError");
        }

        return PublishInitiationResult.Success(id, PublishingJson.ExtractString(body, "permalink_url"));
    }

    private static PublishInitiationResult MapGraphFailure(System.Net.HttpStatusCode statusCode, string body)
    {
        var category = statusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized => "AuthenticationError",
            System.Net.HttpStatusCode.Forbidden => "AuthorizationError",
            System.Net.HttpStatusCode.TooManyRequests => "RateLimitError",
            System.Net.HttpStatusCode.BadRequest => "ValidationError",
            System.Net.HttpStatusCode.InternalServerError => "TransientError",
            _ => "UnknownError"
        };

        return PublishInitiationResult.Failure(
            $"Meta Graph API loi HTTP {(int)statusCode}: {body}",
            $"META_HTTP_{(int)statusCode}", category,
            category is "TransientError" or "RateLimitError");
    }

    private sealed class MetaMediaManifest
    {
        public string? Caption { get; set; }
        public string? LinkUrl { get; set; }
        public string? ImageUrl { get; set; }
        public string? VideoUrl { get; set; }
    }
}

internal static class PublishingValidation
{
    public static ValidationResult ActiveOAuthAccount(ConnectedAccount? account, string provider)
    {
        if (account == null) return ValidationResult.Failure($"Chua lien ket tai khoan {provider}.");
        if (account.Status != CredentialStatus.Active)
            return ValidationResult.Failure($"Tai khoan {provider} chua san sang (trang thai: {account.Status}).");
        if (account.OAuthCredential == null)
            return ValidationResult.Failure($"Tai khoan {provider} chua co OAuth credential.");
        return ValidationResult.Success();
    }

    public static string DecryptAccessToken(ConnectedAccount account, ICredentialVault vault)
    {
        if (account.OAuthCredential == null
            || string.IsNullOrWhiteSpace(account.OAuthCredential.AccessTokenEncrypted)
            || string.IsNullOrWhiteSpace(account.OAuthCredential.EncryptionKeyVersion))
        {
            throw new InvalidOperationException("Khong tim thay OAuth credential.");
        }

        return vault.Decrypt(
            account.OAuthCredential.AccessTokenEncrypted,
            account.OAuthCredential.EncryptionKeyVersion);
    }
}

internal static class PublishingJson
{
    public static T? Parse<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return default;
        try { return JsonSerializer.Deserialize<T>(json, Options); }
        catch { return default; }
    }

    public static string? ExtractString(string json, string propertyName)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty(propertyName, out var prop) ? prop.GetString() : null;
        }
        catch { return null; }
    }

    public static List<string> ParseScopes(string? scopesJson)
        => YouTubeConnectorSupport.ParseScopes(scopesJson);

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
