namespace RealSync.Shared.DTOs.Responses.Publishing;

public record TikTokOAuthAuthorizeResponse(string AuthorizeUrl, string State);

public record TikTokCreatorInfoResponse(
    string? CreatorUsername,
    string? CreatorNickname,
    string? CreatorAvatarUrl,
    IReadOnlyList<string> PrivacyLevelOptions,
    bool CommentDisabled,
    bool DuetDisabled,
    bool StitchDisabled,
    int MaxVideoPostDurationSec,
    bool IsAppAudited,
    string? RestrictionNote
);

public record ChannelCapabilitiesResponse(
    bool SupportsDirectPublish,
    bool SupportsDraftUpload,
    bool SupportsScheduling,
    bool SupportsVideo,
    bool SupportsImages,
    bool RequiresFinalUserConfirmation,
    bool IsAppAudited,
    string? RestrictionReason,
    IReadOnlyList<string> GrantedScopes
);
