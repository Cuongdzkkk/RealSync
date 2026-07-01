using System;
using RealSync.Core.Enums;

namespace RealSync.Shared.DTOs.Responses.Publishing;

public record PublicationJobResponse(
    Guid Id,
    Guid PostId,
    Guid ContentVariantId,
    Guid? ConnectedAccountId,
    PublishMode PublishMode,
    DateTime? ScheduledAtUtc,
    PublicationJobStatus Status,
    string IdempotencyKey,
    string? ExternalPostId,
    string? PublishedUrl,
    DateTime? PublishedAt,
    string? LastErrorMessage,
    DateTime CreatedAt
);
