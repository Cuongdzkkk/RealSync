using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RealSync.Core.Entities;
using RealSync.Core.Enums;
using RealSync.Core.Interfaces.Publishing;
using RealSync.Core.Models.Publishing;

namespace RealSync.Services.Publishing;

/// <summary>
/// Connector YouTube theo official YouTube Data API v3.
/// Dung resumable upload va poll processingDetails, khong danh dau Published khi YouTube con xu ly.
/// </summary>
public class YouTubeConnector : IPublishingConnector
{
    private readonly HttpClient _httpClient;
    private readonly ICredentialVault _vault;
    private readonly ILogger<YouTubeConnector> _logger;

    private const string UploadEndpoint = "https://www.googleapis.com/upload/youtube/v3/videos?uploadType=resumable&part=snippet,status";
    private const string StatusEndpoint = "https://www.googleapis.com/youtube/v3/videos?part=status,processingDetails&id=";

    public YouTubeConnector(HttpClient httpClient, ICredentialVault vault, ILogger<YouTubeConnector> logger)
    {
        _httpClient = httpClient;
        _vault = vault;
        _logger = logger;
    }

    public PublishingChannelType ChannelType => PublishingChannelType.YouTube;

    public Task<ChannelCapabilities> GetCapabilitiesAsync(ConnectedAccount? account, CancellationToken cancellationToken)
    {
        var scopes = YouTubeConnectorSupport.ParseScopes(account?.GrantedScopesJson);
        var hasUploadScope = scopes.Count == 0
            || YouTubeConnectorSupport.HasScope(scopes, "https://www.googleapis.com/auth/youtube.upload")
            || YouTubeConnectorSupport.HasScope(scopes, "https://www.googleapis.com/auth/youtube")
            || YouTubeConnectorSupport.HasScope(scopes, "https://www.googleapis.com/auth/youtube.force-ssl");

        return Task.FromResult(new ChannelCapabilities
        {
            SupportsDirectPublish = account?.Status == CredentialStatus.Active && hasUploadScope,
            SupportsDraftUpload = false,
            SupportsScheduling = true,
            SupportsVideo = true,
            SupportsImages = true,
            SupportsUpdate = false,
            SupportsDelete = false,
            RequiresFinalUserConfirmation = false,
            GrantedScopes = scopes,
            RestrictionReason = account?.Status == CredentialStatus.Active && hasUploadScope
                ? null
                : "Chua ket noi YouTube OAuth hoac thieu scope youtube.upload."
        });
    }

    public Task<ValidationResult> ValidateAsync(PublicationContext context, CancellationToken cancellationToken)
    {
        var accountValidation = ValidateActiveAccount(context.Account, "YouTube");
        if (!accountValidation.IsValid) return Task.FromResult(accountValidation);

        if (string.IsNullOrWhiteSpace(context.Variant.Title))
            return Task.FromResult(ValidationResult.Failure("YouTube yeu cau title video."));

        var manifest = YouTubeConnectorSupport.ParseManifest(context.Job.MediaManifestJson);
        if (manifest == null || string.IsNullOrWhiteSpace(manifest.VideoUrl))
            return Task.FromResult(ValidationResult.Failure("Thieu videoUrl trong media manifest cho YouTube."));

        if (context.Job.PublishMode != PublishMode.Direct)
            return Task.FromResult(ValidationResult.Failure("YouTube connector chi ho tro Direct upload qua official API."));

        return Task.FromResult(ValidationResult.Success());
    }

    public async Task<PublishInitiationResult> PublishAsync(PublicationContext context, CancellationToken cancellationToken)
    {
        var manifest = YouTubeConnectorSupport.ParseManifest(context.Job.MediaManifestJson);
        if (manifest == null || string.IsNullOrWhiteSpace(manifest.VideoUrl))
        {
            return PublishInitiationResult.Failure(
                "Thieu videoUrl trong media manifest cho YouTube.",
                "MISSING_VIDEO_URL", "ValidationError", isRetryable: false);
        }

        string accessToken;
        try
        {
            accessToken = DecryptAccessToken(context.Account!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Khong the giai ma access token YouTube cho account {AccountId}", context.Account!.Id);
            return PublishInitiationResult.Failure(
                "Khong the giai ma access token YouTube. Vui long ket noi lai.",
                "DECRYPTION_FAILED", "AuthenticationError", isRetryable: false);
        }

        using var initRequest = new HttpRequestMessage(HttpMethod.Post, UploadEndpoint);
        initRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        initRequest.Headers.Add("X-Upload-Content-Type", manifest.ContentType ?? "video/*");
        initRequest.Content = new StringContent(
            YouTubeConnectorSupport.BuildVideoResourceJson(context.Variant, manifest),
            Encoding.UTF8,
            "application/json");

        using var initResponse = await _httpClient.SendAsync(initRequest, cancellationToken);
        var initBody = await initResponse.Content.ReadAsStringAsync(cancellationToken);
        if (!initResponse.IsSuccessStatusCode)
            return MapHttpFailure("YOUTUBE_INIT_FAILED", initResponse.StatusCode, initBody);

        var uploadLocation = initResponse.Headers.Location?.ToString();
        if (string.IsNullOrWhiteSpace(uploadLocation))
        {
            return PublishInitiationResult.Failure(
                "YouTube khong tra ve resumable upload URL.",
                "MISSING_UPLOAD_LOCATION", "ProviderContractError", isRetryable: false);
        }

        using var mediaResponse = await _httpClient.GetAsync(manifest.VideoUrl, cancellationToken);
        if (!mediaResponse.IsSuccessStatusCode)
            return MapHttpFailure("YOUTUBE_MEDIA_DOWNLOAD_FAILED", mediaResponse.StatusCode, "Khong tai duoc videoUrl.");

        await using var mediaStream = await mediaResponse.Content.ReadAsStreamAsync(cancellationToken);
        using var uploadRequest = new HttpRequestMessage(HttpMethod.Put, uploadLocation);
        uploadRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        uploadRequest.Content = new StreamContent(mediaStream);
        uploadRequest.Content.Headers.ContentType = new MediaTypeHeaderValue(
            mediaResponse.Content.Headers.ContentType?.MediaType ?? manifest.ContentType ?? "video/mp4");
        if (mediaResponse.Content.Headers.ContentLength.HasValue)
            uploadRequest.Content.Headers.ContentLength = mediaResponse.Content.Headers.ContentLength.Value;

        using var uploadResponse = await _httpClient.SendAsync(uploadRequest, cancellationToken);
        var uploadBody = await uploadResponse.Content.ReadAsStringAsync(cancellationToken);
        if (!uploadResponse.IsSuccessStatusCode)
            return MapHttpFailure("YOUTUBE_UPLOAD_FAILED", uploadResponse.StatusCode, uploadBody);

        var videoId = YouTubeConnectorSupport.ExtractString(uploadBody, "id");
        if (string.IsNullOrWhiteSpace(videoId))
        {
            return PublishInitiationResult.Failure(
                "YouTube upload thanh cong nhung khong tra ve video id.",
                "MISSING_VIDEO_ID", "ProviderContractError");
        }

        context.Job.RemoteStatus = "UPLOADED_PROCESSING";
        return PublishInitiationResult.RemoteProcessing(videoId, context.Job.RemoteStatus);
    }

    public async Task<RemotePublicationStatus> GetStatusAsync(PublicationTrackingContext context, CancellationToken cancellationToken)
    {
        if (context.Account == null || string.IsNullOrWhiteSpace(context.Job.ExternalPublishId))
        {
            return new RemotePublicationStatus
            {
                Status = PublicationJobStatus.NeedsReview,
                ErrorMessage = "Thieu YouTube account hoac video id."
            };
        }

        string accessToken;
        try
        {
            accessToken = DecryptAccessToken(context.Account);
        }
        catch
        {
            return new RemotePublicationStatus
            {
                Status = PublicationJobStatus.NeedsReview,
                ErrorMessage = "Khong the giai ma access token YouTube."
            };
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, StatusEndpoint + Uri.EscapeDataString(context.Job.ExternalPublishId));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new RemotePublicationStatus
            {
                Status = PublicationJobStatus.NeedsReview,
                ErrorMessage = $"YouTube status API loi HTTP {(int)response.StatusCode}."
            };
        }

        return MapStatus(body, context.Job.ExternalPublishId);
    }

    public Task<DeleteRemoteResult> DeleteAsync(PublicationTrackingContext context, CancellationToken cancellationToken)
    {
        return Task.FromResult(DeleteRemoteResult.Failure(
            "YouTube delete chua duoc bat trong Phase 8. Vui long xoa tren YouTube Studio neu can."));
    }

    public static RemotePublicationStatus MapStatus(string responseBody, string videoId)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            var item = doc.RootElement.GetProperty("items").EnumerateArray().FirstOrDefault();
            if (item.ValueKind == JsonValueKind.Undefined)
            {
                return new RemotePublicationStatus
                {
                    Status = PublicationJobStatus.NeedsReview,
                    ErrorMessage = "YouTube khong tim thay video trong response status."
                };
            }

            var uploadStatus = item.TryGetProperty("status", out var status)
                ? GetString(status, "uploadStatus")
                : null;
            var processingStatus = item.TryGetProperty("processingDetails", out var processing)
                ? GetString(processing, "processingStatus")
                : null;

            if (uploadStatus is "rejected" or "failed" || processingStatus == "failed")
            {
                return new RemotePublicationStatus
                {
                    Status = PublicationJobStatus.Failed,
                    ErrorMessage = $"YouTube xu ly that bai. uploadStatus={uploadStatus}, processingStatus={processingStatus}"
                };
            }

            if (processingStatus == "succeeded" || uploadStatus == "processed")
            {
                return new RemotePublicationStatus
                {
                    Status = PublicationJobStatus.Published,
                    PublishedUrl = $"https://www.youtube.com/watch?v={videoId}",
                    RemoteStatusDescription = "YouTube da xu ly video thanh cong."
                };
            }

            return new RemotePublicationStatus
            {
                Status = PublicationJobStatus.RemoteProcessing,
                RemoteStatusDescription = $"YouTube dang xu ly video. uploadStatus={uploadStatus}, processingStatus={processingStatus}"
            };
        }
        catch (JsonException)
        {
            return new RemotePublicationStatus
            {
                Status = PublicationJobStatus.NeedsReview,
                ErrorMessage = "Response status YouTube khong hop le."
            };
        }
    }

    private string DecryptAccessToken(ConnectedAccount account)
    {
        if (account.OAuthCredential == null
            || string.IsNullOrWhiteSpace(account.OAuthCredential.AccessTokenEncrypted)
            || string.IsNullOrWhiteSpace(account.OAuthCredential.EncryptionKeyVersion))
        {
            throw new InvalidOperationException("Khong tim thay OAuth credential.");
        }

        return _vault.Decrypt(
            account.OAuthCredential.AccessTokenEncrypted,
            account.OAuthCredential.EncryptionKeyVersion);
    }

    private static ValidationResult ValidateActiveAccount(ConnectedAccount? account, string provider)
    {
        if (account == null)
            return ValidationResult.Failure($"Chua lien ket tai khoan {provider}.");
        if (account.Status != CredentialStatus.Active)
            return ValidationResult.Failure($"Tai khoan {provider} chua san sang (trang thai: {account.Status}).");
        if (account.OAuthCredential == null)
            return ValidationResult.Failure($"Tai khoan {provider} chua co OAuth credential.");
        return ValidationResult.Success();
    }

    private static string? GetString(JsonElement element, string propertyName)
        => element.TryGetProperty(propertyName, out var prop) ? prop.GetString() : null;

    private static PublishInitiationResult MapHttpFailure(string code, System.Net.HttpStatusCode statusCode, string body)
    {
        var category = statusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized => "AuthenticationError",
            System.Net.HttpStatusCode.Forbidden => "AuthorizationError",
            System.Net.HttpStatusCode.TooManyRequests => "RateLimitError",
            System.Net.HttpStatusCode.BadRequest => "ValidationError",
            System.Net.HttpStatusCode.InternalServerError => "TransientError",
            System.Net.HttpStatusCode.ServiceUnavailable => "TransientError",
            _ => "UnknownError"
        };

        return PublishInitiationResult.Failure(
            $"YouTube API loi HTTP {(int)statusCode}: {body}",
            code, category,
            category is "TransientError" or "RateLimitError");
    }

}
