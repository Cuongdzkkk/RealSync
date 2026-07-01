using System;
using RealSync.Core.Enums;

namespace RealSync.Shared.DTOs.Responses.Publishing;

public record ConnectedAccountResponse(
    Guid Id,
    string Provider,
    PublishingChannelType ChannelType,
    string DisplayName,
    string ExternalAccountId,
    string? ExternalParentAccountId,
    string? ProfileUrl,
    string? AvatarUrl,
    CredentialStatus Status,
    string? GrantedScopesJson,
    DateTime? TokenExpiresAt,
    DateTime? LastValidatedAt,
    string? LastErrorCode,
    string? LastErrorMessage,
    DateTime CreatedAt
);
