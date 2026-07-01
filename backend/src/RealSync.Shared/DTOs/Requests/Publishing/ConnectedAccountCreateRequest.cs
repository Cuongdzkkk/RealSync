using System;
using RealSync.Core.Enums;

namespace RealSync.Shared.DTOs.Requests.Publishing;

public record ConnectedAccountCreateRequest(
    string Provider,
    PublishingChannelType ChannelType,
    string DisplayName,
    string ExternalAccountId,
    string? ExternalParentAccountId,
    string? ProfileUrl,
    string? AvatarUrl,
    string AccessToken,
    string? RefreshToken,
    int? ExpiresInSeconds,
    string? GrantedScopesJson
);
