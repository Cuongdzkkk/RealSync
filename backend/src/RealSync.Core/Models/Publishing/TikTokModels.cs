using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RealSync.Core.Models.Publishing;

/// <summary>
/// Media manifest cho publication job TikTok (lưu trong PublicationJob.MediaManifestJson).
/// </summary>
public class TikTokMediaManifest
{
    public string? VideoUrl { get; set; }
    public long? VideoSizeBytes { get; set; }
    public int? DurationSeconds { get; set; }
    public bool IsAigc { get; set; }
    public string? PrivacyLevel { get; set; }
    public bool UserConsentConfirmed { get; set; }
    public bool? DisableComment { get; set; }
    public bool? DisableDuet { get; set; }
    public bool? DisableStitch { get; set; }
    public int? VideoCoverTimestampMs { get; set; }
}

/// <summary>
/// Thông tin creator từ TikTok creator_info/query API.
/// </summary>
public class TikTokCreatorInfo
{
    public string? CreatorUsername { get; set; }
    public string? CreatorNickname { get; set; }
    public string? CreatorAvatarUrl { get; set; }
    public List<string> PrivacyLevelOptions { get; set; } = new();
    public bool CommentDisabled { get; set; }
    public bool DuetDisabled { get; set; }
    public bool StitchDisabled { get; set; }
    public int MaxVideoPostDurationSec { get; set; }
}

/// <summary>
/// Kết quả parse response TikTok API.
/// </summary>
public class TikTokApiResult<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string? LogId { get; set; }
    public int? HttpStatus { get; set; }
}

public class TikTokPublishInitData
{
    [JsonPropertyName("publish_id")]
    public string? PublishId { get; set; }

    [JsonPropertyName("upload_url")]
    public string? UploadUrl { get; set; }
}

public class TikTokStatusData
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("fail_reason")]
    public string? FailReason { get; set; }

    [JsonPropertyName("publicaly_available_post_id")]
    public List<string>? PubliclyAvailablePostIds { get; set; }
}
