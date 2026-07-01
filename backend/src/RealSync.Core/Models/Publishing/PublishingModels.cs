using System.Collections.Generic;
using RealSync.Core.Entities;
using RealSync.Core.Enums;

namespace RealSync.Core.Models.Publishing;

public class ChannelCapabilities
{
    public bool SupportsDirectPublish { get; set; } = true;
    public bool SupportsDraftUpload { get; set; } = false;
    public bool SupportsScheduling { get; set; } = true;
    public bool SupportsVideo { get; set; } = false;
    public bool SupportsImages { get; set; } = true;
    public bool SupportsUpdate { get; set; } = false;
    public bool SupportsDelete { get; set; } = false;
    public bool RequiresFinalUserConfirmation { get; set; } = false;
    public bool IsAppAudited { get; set; }
    public string? RestrictionReason { get; set; }
    public List<string> GrantedScopes { get; set; } = new();
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ValidationResult Success() => new() { IsValid = true };
    public static ValidationResult Failure(params string[] errors) => new() { IsValid = false, Errors = new(errors) };
}

public class PublishInitiationResult
{
    public bool IsSuccess { get; set; }
    public string? ExternalPostId { get; set; }
    public string? ExternalPublishId { get; set; }
    public string? PublishedUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorCategory { get; set; }
    public bool RequiresRemoteProcessing { get; set; }
    public bool IsRetryable { get; set; } = true;

    public static PublishInitiationResult Success(string? externalPostId, string? publishedUrl) 
        => new() { IsSuccess = true, ExternalPostId = externalPostId, PublishedUrl = publishedUrl };

    public static PublishInitiationResult RemoteProcessing(string externalPublishId, string? remoteStatus = null)
        => new()
        {
            IsSuccess = true,
            RequiresRemoteProcessing = true,
            ExternalPublishId = externalPublishId,
            ExternalPostId = remoteStatus
        };

    public static PublishInitiationResult Failure(
        string errorMessage,
        string? errorCode = null,
        string? errorCategory = null,
        bool isRetryable = true)
        => new()
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode,
            ErrorCategory = errorCategory,
            IsRetryable = isRetryable
        };
}

public class RemotePublicationStatus
{
    public PublicationJobStatus Status { get; set; }
    public string? RemoteStatusDescription { get; set; }
    public string? PublishedUrl { get; set; }
    public string? ErrorMessage { get; set; }
}

public class DeleteRemoteResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }

    public static DeleteRemoteResult Success() => new() { IsSuccess = true };
    public static DeleteRemoteResult Failure(string errorMessage) => new() { IsSuccess = false, ErrorMessage = errorMessage };
}

public class PublicationContext
{
    public PublicationJob Job { get; set; } = null!;
    public ContentVariant Variant { get; set; } = null!;
    public ConnectedAccount? Account { get; set; }
}

public class PublicationTrackingContext
{
    public PublicationJob Job { get; set; } = null!;
    public ConnectedAccount? Account { get; set; }
}
