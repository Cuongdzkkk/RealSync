namespace RealSync.Services.Options;

/// <summary>
/// Cấu hình TikTok Login Kit và Content Posting API.
/// </summary>
public class TikTokOptions
{
    public const string SectionName = "TikTok";

    public string ClientKey { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;

    /// <summary>
    /// True khi app đã pass Content Posting API audit — cho phép direct post công khai.
    /// </summary>
    public bool IsAppAudited { get; set; }

    /// <summary>
    /// Secret dùng xác minh webhook signature từ TikTok.
    /// </summary>
    public string? WebhookSecret { get; set; }

    public string ApiBaseUrl { get; set; } = "https://open.tiktokapis.com";
    public string AuthorizeUrl { get; set; } = "https://www.tiktok.com/v2/auth/authorize/";

    public string DefaultScopes { get; set; } = "user.info.basic,video.upload,video.publish";
}
