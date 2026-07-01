using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RealSync.Core.Entities;
using RealSync.Core.Enums;
using RealSync.Core.Interfaces.Publishing;
using RealSync.Core.Models.Publishing;

namespace RealSync.Services.Publishing;

/// <summary>
/// Connector Instagram Professional theo Meta content publishing API.
/// Video container duoc poll truoc khi publish de tranh fake success.
/// </summary>
public class InstagramProfessionalConnector : IPublishingConnector
{
    private readonly HttpClient _httpClient;
    private readonly ICredentialVault _vault;
    private readonly ILogger<InstagramProfessionalConnector> _logger;

    public InstagramProfessionalConnector(
        HttpClient httpClient,
        ICredentialVault vault,
        ILogger<InstagramProfessionalConnector> logger)
    {
        _httpClient = httpClient;
        _vault = vault;
        _logger = logger;
    }

    public PublishingChannelType ChannelType => PublishingChannelType.InstagramProfessional;

    public Task<ChannelCapabilities> GetCapabilitiesAsync(ConnectedAccount? account, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ChannelCapabilities
        {
            SupportsDirectPublish = account?.Status == CredentialStatus.Active,
            SupportsDraftUpload = false,
            SupportsScheduling = false,
            SupportsVideo = true,
            SupportsImages = true,
            SupportsUpdate = false,
            SupportsDelete = false,
            RequiresFinalUserConfirmation = false,
            GrantedScopes = PublishingJson.ParseScopes(account?.GrantedScopesJson),
            RestrictionReason = account?.Status == CredentialStatus.Active
                ? null
                : "Chua ket noi Instagram Professional account hop le."
        });
    }

    public Task<ValidationResult> ValidateAsync(PublicationContext context, CancellationToken cancellationToken)
    {
        var accountValidation = PublishingValidation.ActiveOAuthAccount(context.Account, "Instagram Professional");
        if (!accountValidation.IsValid) return Task.FromResult(accountValidation);

        var manifest = PublishingJson.Parse<InstagramMediaManifest>(context.Job.MediaManifestJson);
        if (manifest == null || (string.IsNullOrWhiteSpace(manifest.ImageUrl) && string.IsNullOrWhiteSpace(manifest.VideoUrl)))
            return Task.FromResult(ValidationResult.Failure("Instagram yeu cau imageUrl hoac videoUrl trong media manifest."));

        if (context.Job.PublishMode != PublishMode.Direct)
            return Task.FromResult(ValidationResult.Failure("Instagram connector chi ho tro Direct qua official API."));

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
            _logger.LogError(ex, "Khong the giai ma Instagram token cho account {AccountId}", context.Account!.Id);
            return PublishInitiationResult.Failure(
                "Khong the giai ma Instagram access token. Vui long ket noi lai.",
                "DECRYPTION_FAILED", "AuthenticationError", isRetryable: false);
        }

        var manifest = PublishingJson.Parse<InstagramMediaManifest>(context.Job.MediaManifestJson)!;
        var igUserId = context.Account!.ExternalAccountId;
        var isVideo = !string.IsNullOrWhiteSpace(manifest.VideoUrl);
        var createResult = await PostFormAsync(
            $"{igUserId}/media",
            new Dictionary<string, string?>
            {
                ["image_url"] = isVideo ? null : manifest.ImageUrl,
                ["video_url"] = isVideo ? manifest.VideoUrl : null,
                ["media_type"] = isVideo ? manifest.MediaType ?? "REELS" : null,
                ["caption"] = manifest.Caption ?? context.Variant.Caption,
                ["access_token"] = accessToken
            },
            cancellationToken);

        if (!createResult.IsSuccess) return createResult;
        var containerId = createResult.ExternalPostId!;

        if (isVideo)
        {
            context.Job.RemoteStatus = "IG_CONTAINER_CREATED";
            return PublishInitiationResult.RemoteProcessing(containerId, context.Job.RemoteStatus);
        }

        return await PublishContainerAsync(igUserId, containerId, accessToken, cancellationToken);
    }

    public async Task<RemotePublicationStatus> GetStatusAsync(PublicationTrackingContext context, CancellationToken cancellationToken)
    {
        if (context.Account == null || string.IsNullOrWhiteSpace(context.Job.ExternalPublishId))
        {
            return new RemotePublicationStatus
            {
                Status = PublicationJobStatus.NeedsReview,
                ErrorMessage = "Thieu Instagram account hoac container id."
            };
        }

        var accessToken = PublishingValidation.DecryptAccessToken(context.Account, _vault);
        using var statusRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"{context.Job.ExternalPublishId}?fields=status_code&access_token={Uri.EscapeDataString(accessToken)}");
        using var statusResponse = await _httpClient.SendAsync(statusRequest, cancellationToken);
        var statusBody = await statusResponse.Content.ReadAsStringAsync(cancellationToken);
        if (!statusResponse.IsSuccessStatusCode)
        {
            return new RemotePublicationStatus
            {
                Status = PublicationJobStatus.NeedsReview,
                ErrorMessage = $"Instagram status API loi HTTP {(int)statusResponse.StatusCode}."
            };
        }

        var status = PublishingJson.ExtractString(statusBody, "status_code");
        if (status == "FINISHED")
        {
            var publishResult = await PublishContainerAsync(
                context.Account.ExternalAccountId,
                context.Job.ExternalPublishId,
                accessToken,
                cancellationToken);

            return publishResult.IsSuccess
                ? new RemotePublicationStatus
                {
                    Status = PublicationJobStatus.Published,
                    RemoteStatusDescription = "Instagram media da publish thanh cong."
                }
                : new RemotePublicationStatus
                {
                    Status = PublicationJobStatus.NeedsReview,
                    ErrorMessage = publishResult.ErrorMessage
                };
        }

        if (status == "ERROR" || status == "EXPIRED")
        {
            return new RemotePublicationStatus
            {
                Status = PublicationJobStatus.Failed,
                ErrorMessage = $"Instagram container status: {status}"
            };
        }

        return new RemotePublicationStatus
        {
            Status = PublicationJobStatus.RemoteProcessing,
            RemoteStatusDescription = $"Instagram container status: {status ?? "unknown"}"
        };
    }

    public Task<DeleteRemoteResult> DeleteAsync(PublicationTrackingContext context, CancellationToken cancellationToken)
    {
        return Task.FromResult(DeleteRemoteResult.Failure(
            "Instagram delete chua duoc bat trong Phase 8."));
    }

    private async Task<PublishInitiationResult> PublishContainerAsync(
        string igUserId,
        string creationId,
        string accessToken,
        CancellationToken cancellationToken)
    {
        return await PostFormAsync(
            $"{igUserId}/media_publish",
            new Dictionary<string, string?>
            {
                ["creation_id"] = creationId,
                ["access_token"] = accessToken
            },
            cancellationToken);
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
        {
            return PublishInitiationResult.Failure(
                $"Instagram Graph API loi HTTP {(int)response.StatusCode}: {body}",
                $"INSTAGRAM_HTTP_{(int)response.StatusCode}",
                response.StatusCode == System.Net.HttpStatusCode.TooManyRequests ? "RateLimitError" : "ProviderError",
                response.StatusCode == System.Net.HttpStatusCode.TooManyRequests);
        }

        var id = PublishingJson.ExtractString(body, "id");
        if (string.IsNullOrWhiteSpace(id))
        {
            return PublishInitiationResult.Failure(
                "Instagram Graph API khong tra ve id.",
                "MISSING_INSTAGRAM_ID", "ProviderContractError");
        }

        return PublishInitiationResult.Success(id, null);
    }

    private sealed class InstagramMediaManifest
    {
        public string? Caption { get; set; }
        public string? ImageUrl { get; set; }
        public string? VideoUrl { get; set; }
        public string? MediaType { get; set; }
    }
}
