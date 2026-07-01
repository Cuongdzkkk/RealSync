using System;
using RealSync.Core.Enums;

namespace RealSync.Shared.DTOs.Requests.Publishing;

public record QueuePublicationRequest(
    Guid PostId,
    Guid ContentVariantId,
    Guid? ConnectedAccountId,
    PublishMode PublishMode = PublishMode.Direct,
    DateTime? ScheduledAtUtc = null,
    string? MediaManifestJson = null
);
