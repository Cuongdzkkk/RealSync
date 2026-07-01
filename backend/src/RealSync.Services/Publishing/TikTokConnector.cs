using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RealSync.Core.Entities;
using RealSync.Core.Enums;
using RealSync.Core.Interfaces.Publishing;
using RealSync.Core.Models.Publishing;
using RealSync.Services.Options;

namespace RealSync.Services.Publishing;

/// <summary>
/// Connector TikTok theo official Content Posting API.
/// Hỗ trợ Upload/Draft (video.upload) và Direct Post (video.publish) khi app đã audit.
/// </summary>
public class TikTokConnector : IPublishingConnector
{
    private readonly HttpClient _httpClient;
    private readonly ICredentialVault _vault;
    private readonly TikTokOptions _options;
    private readonly ILogger<TikTokConnector> _logger;

    private const string CreatorInfoPath = "v2/post/publish/creator_info/query/";
    private const string DirectPostInitPath = "v2/post/publish/video/init/";
    private const string DraftUploadInitPath = "v2/post/publish/inbox/video/init/";
    private const string StatusFetchPath = "v2/post/publish/status/fetch/";

    private static readonly HashSet<string> NonRetryableErrorCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "spam_risk", "spam_risk_too_many_posts", "spam_risk_user_banned_from_posting",
        "spam_risk_text", "auth_removed", "scope_not_authorized", "access_token_invalid",
        "unaudited_client_can_only_post_to_private_accounts", "privacy_level_option_mismatch",
        "publish_cancelled", "file_format_check_failed", "duration_check_failed",
        "frame_rate_check_failed", "picture_size_check_failed"
    };

    public TikTokConnector(
        HttpClient httpClient,
        ICredentialVault vault,
        IOptions<TikTokOptions> options,
        ILogger<TikTokConnector> logger)
    {
        _httpClient = httpClient;
        _vault = vault;
        _options = options.Value;
        _logger = logger;
    }

    public PublishingChannelType ChannelType => PublishingChannelType.TikTok;

    public Task<ChannelCapabilities> GetCapabilitiesAsync(ConnectedAccount? account, CancellationToken cancellationToken)
    {
        var scopes = ParseScopes(account?.GrantedScopesJson);
        var hasUpload = HasScope(scopes, "video.upload");
        var hasPublish = HasScope(scopes, "video.publish");
        var isAudited = _options.IsAppAudited;

        var caps = new ChannelCapabilities
        {
            SupportsVideo = true,
            SupportsImages = false,
            SupportsScheduling = false,
            SupportsUpdate = false,
            SupportsDelete = false,
            RequiresFinalUserConfirmation = true,
            IsAppAudited = isAudited,
            GrantedScopes = scopes
        };

        if (account == null || account.Status != CredentialStatus.Active)
        {
            caps.SupportsDirectPublish = false;
            caps.SupportsDraftUpload = false;
            caps.RestrictionReason = "Chưa liên kết tài khoản TikTok hoặc token chưa sẵn sàng.";
            return Task.FromResult(caps);
        }

        caps.SupportsDraftUpload = hasUpload;
        caps.SupportsDirectPublish = hasPublish && isAudited;

        if (!isAudited)
        {
            caps.RestrictionReason =
                "App TikTok chưa qua Content Posting API audit. Chỉ hỗ trợ Upload/Draft (video.upload) — " +
                "người dùng hoàn tất đăng trên TikTok. Direct post công khai sẽ khả dụng sau khi audit.";
            if (!hasUpload)
            {
                caps.SupportsDraftUpload = false;
                caps.RestrictionReason = "Thiếu scope video.upload. Vui lòng kết nối lại với đủ quyền.";
            }
        }
        else if (!hasPublish)
        {
            caps.RestrictionReason = "Thiếu scope video.publish. Vui lòng kết nối lại OAuth với quyền đăng video.";
        }

        return Task.FromResult(caps);
    }

    public async Task<ValidationResult> ValidateAsync(PublicationContext context, CancellationToken cancellationToken)
    {
        if (context.Account == null)
            return ValidationResult.Failure("Chưa liên kết tài khoản TikTok. Vui lòng kết nối tại Cài đặt hệ thống → Tài khoản liên kết.");

        if (context.Account.Status == CredentialStatus.PendingSetup)
            return ValidationResult.Failure("Tài khoản TikTok chưa được thiết lập. Vui lòng hoàn thành OAuth.");

        if (context.Account.Status == CredentialStatus.Expired)
            return ValidationResult.Failure("Token TikTok đã hết hạn. Vui lòng kết nối lại.");

        if (context.Account.Status is CredentialStatus.Revoked or CredentialStatus.Invalid)
            return ValidationResult.Failure($"Tài khoản TikTok không hợp lệ (trạng thái: {context.Account.Status}).");

        if (context.Account.Status != CredentialStatus.Active)
            return ValidationResult.Failure($"Tài khoản TikTok chưa sẵn sàng (trạng thái: {context.Account.Status}).");

        var manifest = ParseMediaManifest(context.Job.MediaManifestJson);
        if (manifest == null || string.IsNullOrWhiteSpace(manifest.VideoUrl))
            return ValidationResult.Failure("Thiếu video URL trong media manifest. TikTok yêu cầu video 9:16 với URL công khai đã verify.");

        if (manifest != null && !manifest.UserConsentConfirmed)
            return ValidationResult.Failure("Người dùng chưa xác nhận đồng ý gửi video lên TikTok.");

        if (manifest != null && string.IsNullOrWhiteSpace(manifest.PrivacyLevel))
            return ValidationResult.Failure("Chưa chọn mức riêng tư (privacy level). Người dùng phải chọn thủ công, không có giá trị mặc định.");

        var caps = await GetCapabilitiesAsync(context.Account, cancellationToken);
        var isDraftMode = context.Job.PublishMode == PublishMode.DraftUpload;

        if (isDraftMode && !caps.SupportsDraftUpload)
            return ValidationResult.Failure(caps.RestrictionReason ?? "Chế độ Upload/Draft không khả dụng.");

        if (!isDraftMode && !caps.SupportsDirectPublish)
        {
            if (!_options.IsAppAudited)
                return ValidationResult.Failure(
                    "App chưa qua TikTok audit — không thể direct post công khai. " +
                    "Hãy dùng chế độ Upload/Draft hoặc hoàn tất audit tại TikTok Developer Portal.");
            return ValidationResult.Failure(caps.RestrictionReason ?? "Direct post không khả dụng.");
        }

        if (manifest != null && manifest.DurationSeconds.HasValue && manifest.DurationSeconds > 600)
            return ValidationResult.Failure("Video vượt quá thời lượng cho phép của TikTok.");

        return ValidationResult.Success();
    }

    public async Task<PublishInitiationResult> PublishAsync(PublicationContext context, CancellationToken cancellationToken)
    {
        string accessToken;
        try
        {
            accessToken = DecryptAccessToken(context.Account!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không thể giải mã access token TikTok cho account {AccountId}", context.Account!.Id);
            return PublishInitiationResult.Failure(
                "Không thể giải mã access token. Vui lòng kết nối lại.",
                "DECRYPTION_FAILED", "AuthenticationError", isRetryable: false);
        }

        var manifest = ParseMediaManifest(context.Job.MediaManifestJson);
        if (manifest == null || string.IsNullOrWhiteSpace(manifest.VideoUrl))
        {
            return PublishInitiationResult.Failure(
                "Thiếu video URL trong media manifest. TikTok yêu cầu video 9:16.",
                "MISSING_VIDEO_URL", "ValidationError", isRetryable: false);
        }

        var isDraftMode = context.Job.PublishMode == PublishMode.DraftUpload;

        if (!isDraftMode)
        {
            var creatorResult = await QueryCreatorInfoAsync(accessToken, cancellationToken);
            if (!creatorResult.IsSuccess)
                return MapApiFailure(creatorResult);

            if (creatorResult.Data != null
                && !creatorResult.Data.PrivacyLevelOptions.Contains(manifest.PrivacyLevel!))
            {
                return PublishInitiationResult.Failure(
                    $"Mức riêng tư '{manifest.PrivacyLevel}' không có trong privacy_level_options của creator.",
                    "PRIVACY_LEVEL_MISMATCH", "ValidationError", isRetryable: false);
            }
        }

        var initPath = isDraftMode ? DraftUploadInitPath : DirectPostInitPath;
        var title = !string.IsNullOrWhiteSpace(context.Variant.Caption)
            ? context.Variant.Caption
            : context.Variant.Title ?? "Video RealSync";
        var initBody = BuildInitBody(manifest, isDraftMode, title);

        var initResult = await PostTikTokApiAsync<TikTokPublishInitData>(
            initPath, initBody, accessToken, cancellationToken);

        if (!initResult.IsSuccess)
            return MapApiFailure(initResult);

        var publishId = initResult.Data?.PublishId;
        if (string.IsNullOrEmpty(publishId))
        {
            return PublishInitiationResult.Failure(
                "TikTok API không trả publish_id.",
                "MISSING_PUBLISH_ID", "UnknownError");
        }

        _logger.LogInformation(
            "TikTok video init thành công. PublishId={PublishId}, Mode={Mode}, AccountId={AccountId}",
            publishId, isDraftMode ? "DraftUpload" : "DirectPost", context.Account!.Id);

        context.Job.RemoteStatus = isDraftMode ? "SEND_TO_USER_INBOX" : "PROCESSING_DOWNLOAD";
        return PublishInitiationResult.RemoteProcessing(publishId, context.Job.RemoteStatus);
    }

    public async Task<RemotePublicationStatus> GetStatusAsync(
        PublicationTrackingContext context, CancellationToken cancellationToken)
    {
        if (context.Account == null || string.IsNullOrEmpty(context.Job.ExternalPublishId))
        {
            return new RemotePublicationStatus
            {
                Status = PublicationJobStatus.NeedsReview,
                ErrorMessage = "Thiếu publish_id hoặc tài khoản TikTok."
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
                ErrorMessage = "Không thể giải mã access token TikTok."
            };
        }

        var body = JsonSerializer.Serialize(new { publish_id = context.Job.ExternalPublishId });
        var result = await PostTikTokApiAsync<TikTokStatusData>(
            StatusFetchPath, body, accessToken, cancellationToken);

        if (!result.IsSuccess)
        {
            return new RemotePublicationStatus
            {
                Status = PublicationJobStatus.NeedsReview,
                ErrorMessage = result.ErrorMessage ?? "Không thể lấy trạng thái từ TikTok."
            };
        }

        return MapRemoteStatus(result.Data, context.Job.PublishMode);
    }

    public Task<DeleteRemoteResult> DeleteAsync(PublicationTrackingContext context, CancellationToken cancellationToken)
    {
        return Task.FromResult(DeleteRemoteResult.Failure(
            "TikTok Content Posting API không hỗ trợ xóa video qua API. Vui lòng xóa trực tiếp trên TikTok."));
    }

    /// <summary>
    /// Query creator info — dùng cho diagnostics và UI privacy picker.
    /// </summary>
    public async Task<TikTokApiResult<TikTokCreatorInfo>> QueryCreatorInfoAsync(
        string accessToken, CancellationToken cancellationToken)
    {
        var result = await PostTikTokApiAsync<TikTokCreatorInfoDto>(
            CreatorInfoPath, "{}", accessToken, cancellationToken);

        if (!result.IsSuccess || result.Data == null)
        {
            return new TikTokApiResult<TikTokCreatorInfo>
            {
                IsSuccess = false,
                ErrorCode = result.ErrorCode,
                ErrorMessage = result.ErrorMessage,
                LogId = result.LogId
            };
        }

        var dto = result.Data;
        return new TikTokApiResult<TikTokCreatorInfo>
        {
            IsSuccess = true,
            Data = new TikTokCreatorInfo
            {
                CreatorUsername = dto.CreatorUsername,
                CreatorNickname = dto.CreatorNickname,
                CreatorAvatarUrl = dto.CreatorAvatarUrl,
                CommentDisabled = dto.CommentDisabled,
                DuetDisabled = dto.DuetDisabled,
                StitchDisabled = dto.StitchDisabled,
                MaxVideoPostDurationSec = dto.MaxVideoPostDurationSec,
                PrivacyLevelOptions = dto.PrivacyLevelOptions ?? new List<string>()
            },
            LogId = result.LogId
        };
    }

    private sealed class TikTokCreatorInfoDto
    {
        [JsonPropertyName("creator_username")]
        public string? CreatorUsername { get; set; }

        [JsonPropertyName("creator_nickname")]
        public string? CreatorNickname { get; set; }

        [JsonPropertyName("creator_avatar_url")]
        public string? CreatorAvatarUrl { get; set; }

        [JsonPropertyName("comment_disabled")]
        public bool CommentDisabled { get; set; }

        [JsonPropertyName("duet_disabled")]
        public bool DuetDisabled { get; set; }

        [JsonPropertyName("stitch_disabled")]
        public bool StitchDisabled { get; set; }

        [JsonPropertyName("max_video_post_duration_sec")]
        public int MaxVideoPostDurationSec { get; set; }

        [JsonPropertyName("privacy_level_options")]
        public List<string>? PrivacyLevelOptions { get; set; }
    }

    #region Private Helpers

    private string DecryptAccessToken(ConnectedAccount account)
    {
        if (account.OAuthCredential == null
            || string.IsNullOrEmpty(account.OAuthCredential.AccessTokenEncrypted)
            || string.IsNullOrEmpty(account.OAuthCredential.EncryptionKeyVersion))
        {
            throw new InvalidOperationException("Không tìm thấy credential OAuth cho tài khoản TikTok.");
        }

        return _vault.Decrypt(
            account.OAuthCredential.AccessTokenEncrypted,
            account.OAuthCredential.EncryptionKeyVersion);
    }

    private static TikTokMediaManifest? ParseMediaManifest(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            return JsonSerializer.Deserialize<TikTokMediaManifest>(json, ManifestJsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private static string BuildInitBody(TikTokMediaManifest manifest, bool isDraftMode, string title)
    {
        var sourceInfo = new Dictionary<string, object>
        {
            ["source"] = "PULL_FROM_URL",
            ["video_url"] = manifest.VideoUrl!
        };

        if (isDraftMode)
        {
            return JsonSerializer.Serialize(new { source_info = sourceInfo }, JsonOptions);
        }

        var postInfo = new Dictionary<string, object?>
        {
            ["title"] = title,
            ["privacy_level"] = manifest.PrivacyLevel,
            ["disable_comment"] = manifest.DisableComment ?? false,
            ["disable_duet"] = manifest.DisableDuet ?? false,
            ["disable_stitch"] = manifest.DisableStitch ?? false,
            ["video_cover_timestamp_ms"] = manifest.VideoCoverTimestampMs ?? 1000,
            ["is_aigc"] = manifest.IsAigc
        };

        return JsonSerializer.Serialize(new { post_info = postInfo, source_info = sourceInfo }, JsonOptions);
    }

    private async Task<TikTokApiResult<T>> PostTikTokApiAsync<T>(
        string path, string jsonBody, string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogDebug("TikTok API {Path} response: {StatusCode}", path, response.StatusCode);

            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            string? errorCode = null;
            string? errorMessage = null;
            string? logId = null;

            if (root.TryGetProperty("error", out var errorProp))
            {
                errorCode = errorProp.TryGetProperty("code", out var codeProp)
                    ? codeProp.GetString()
                    : null;
                errorMessage = errorProp.TryGetProperty("message", out var msgProp)
                    ? msgProp.GetString()
                    : null;
                logId = errorProp.TryGetProperty("log_id", out var logProp)
                    ? logProp.GetString()
                    : null;
            }

            if (!response.IsSuccessStatusCode || (errorCode != null && !string.Equals(errorCode, "ok", StringComparison.OrdinalIgnoreCase)))
            {
                return new TikTokApiResult<T>
                {
                    IsSuccess = false,
                    ErrorCode = errorCode ?? $"HTTP_{(int)response.StatusCode}",
                    ErrorMessage = errorMessage ?? responseBody,
                    LogId = logId,
                    HttpStatus = (int)response.StatusCode
                };
            }

            T? data = default;
            if (root.TryGetProperty("data", out var dataProp))
            {
                data = JsonSerializer.Deserialize<T>(dataProp.GetRawText(), JsonOptions);
            }

            return new TikTokApiResult<T>
            {
                IsSuccess = true,
                Data = data,
                LogId = logId,
                HttpStatus = (int)response.StatusCode
            };
        }
        catch (TaskCanceledException)
        {
            return new TikTokApiResult<T>
            {
                IsSuccess = false,
                ErrorCode = "NETWORK_TIMEOUT",
                ErrorMessage = "Yêu cầu đến TikTok API bị timeout."
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error calling TikTok API {Path}", path);
            return new TikTokApiResult<T>
            {
                IsSuccess = false,
                ErrorCode = "NETWORK_ERROR",
                ErrorMessage = $"Lỗi kết nối đến TikTok API: {ex.Message}"
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse TikTok API response for {Path}", path);
            return new TikTokApiResult<T>
            {
                IsSuccess = false,
                ErrorCode = "INVALID_RESPONSE",
                ErrorMessage = "Phản hồi từ TikTok API không hợp lệ."
            };
        }
    }

    public static PublishInitiationResult MapApiFailure<T>(TikTokApiResult<T> result)
    {
        var code = result.ErrorCode ?? "TIKTOK_API_ERROR";
        var category = MapErrorCategory(code);
        var isRetryable = !NonRetryableErrorCodes.Contains(code)
            && category is "TransientError" or "RateLimitError";

        return PublishInitiationResult.Failure(
            $"TikTok API lỗi ({code}): {result.ErrorMessage}",
            code, category, isRetryable);
    }

    private static RemotePublicationStatus MapRemoteStatus(TikTokStatusData? data, PublishMode mode)
    {
        if (data == null)
        {
            return new RemotePublicationStatus
            {
                Status = PublicationJobStatus.NeedsReview,
                ErrorMessage = "Không có dữ liệu trạng thái từ TikTok."
            };
        }

        return data.Status switch
        {
            "PUBLISH_COMPLETE" => new RemotePublicationStatus
            {
                Status = PublicationJobStatus.Published,
                RemoteStatusDescription = mode == PublishMode.DraftUpload
                    ? "Người dùng đã hoàn tất đăng video trên TikTok."
                    : "Video đã được đăng thành công trên TikTok.",
                PublishedUrl = data.PubliclyAvailablePostIds?.Count > 0
                    ? $"https://www.tiktok.com/@/video/{data.PubliclyAvailablePostIds[0]}"
                    : null
            },
            "SEND_TO_USER_INBOX" => new RemotePublicationStatus
            {
                Status = PublicationJobStatus.RemoteProcessing,
                RemoteStatusDescription = "Video đã upload. Thông báo đã gửi đến inbox TikTok — người dùng cần hoàn tất trên app."
            },
            "PROCESSING_UPLOAD" or "PROCESSING_DOWNLOAD" => new RemotePublicationStatus
            {
                Status = PublicationJobStatus.RemoteProcessing,
                RemoteStatusDescription = $"TikTok đang xử lý video ({data.Status})."
            },
            "FAILED" => new RemotePublicationStatus
            {
                Status = PublicationJobStatus.Failed,
                ErrorMessage = $"TikTok xử lý thất bại: {data.FailReason ?? "unknown"}"
            },
            _ => new RemotePublicationStatus
            {
                Status = PublicationJobStatus.RemoteProcessing,
                RemoteStatusDescription = $"Trạng thái TikTok: {data.Status}"
            }
        };
    }

    private static string MapErrorCategory(string? code)
    {
        if (string.IsNullOrEmpty(code)) return "UnknownError";

        return code switch
        {
            "access_token_invalid" or "token_not_authorized_for_specified_publish_id" => "AuthenticationError",
            "scope_not_authorized" => "AuthorizationError",
            "rate_limit_exceeded" => "RateLimitError",
            "unaudited_client_can_only_post_to_private_accounts" or "privacy_level_option_mismatch" => "ConfigurationError",
            var c when c.StartsWith("spam_risk", StringComparison.OrdinalIgnoreCase) => "PolicyError",
            "internal" or "video_pull_failed" or "photo_pull_failed" => "TransientError",
            _ => "UnknownError"
        };
    }

    private static List<string> ParsePrivacyOptions(JsonElement data)
    {
        var options = new List<string>();
        if (data.TryGetProperty("privacy_level_options", out var arr))
        {
            foreach (var item in arr.EnumerateArray())
            {
                var val = item.GetString();
                if (!string.IsNullOrEmpty(val)) options.Add(val);
            }
        }
        return options;
    }

    private static List<string> ParseScopes(string? scopesJson)
    {
        if (string.IsNullOrWhiteSpace(scopesJson)) return new List<string>();
        if (scopesJson.TrimStart().StartsWith('['))
        {
            try
            {
                return JsonSerializer.Deserialize<List<string>>(scopesJson) ?? new List<string>();
            }
            catch { /* fall through */ }
        }
        return new List<string>(scopesJson.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    private static bool HasScope(List<string> scopes, string scope)
        => scopes.Exists(s => s.Equals(scope, StringComparison.OrdinalIgnoreCase));

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonSerializerOptions ManifestJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    #endregion
}
