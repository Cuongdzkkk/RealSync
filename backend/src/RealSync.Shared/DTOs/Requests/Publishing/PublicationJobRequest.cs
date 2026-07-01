using System;
using RealSync.Core.Enums;

namespace RealSync.Shared.DTOs.Requests.Publishing;

public record PublicationJobRequest(
    Guid PostId,
    Guid ContentVariantId,
    Guid? ConnectedAccountId,
    PublishMode PublishMode,
    DateTime? ScheduledAtUtc,
    int Priority,
    string? MediaManifestJson
);
