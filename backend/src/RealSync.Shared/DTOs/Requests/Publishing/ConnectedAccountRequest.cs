using System;
using RealSync.Core.Enums;

namespace RealSync.Shared.DTOs.Requests.Publishing;

public record ConnectedAccountRequest(
    Guid WorkspaceId,
    string Provider,
    PublishingChannelType ChannelType,
    string DisplayName,
    string ExternalAccountId,
    string? ExternalParentAccountId,
    string? ProfileUrl,
    string? AvatarUrl
);
