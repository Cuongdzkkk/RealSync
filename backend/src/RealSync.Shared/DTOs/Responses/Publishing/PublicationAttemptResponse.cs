using System;

namespace RealSync.Shared.DTOs.Responses.Publishing;

public record PublicationAttemptResponse(
    Guid Id,
    Guid PublicationJobId,
    int AttemptNumber,
    DateTime StartedAt,
    DateTime? CompletedAt,
    long? DurationMs,
    int? ProviderHttpStatus,
    string? ProviderErrorCode,
    string? NormalizedErrorCategory,
    string? ProviderRequestId,
    string? RequestMetadataJson,
    string? ResponseMetadataJson,
    bool IsSuccess,
    string? RetryDecision,
    DateTime CreatedAt
);
