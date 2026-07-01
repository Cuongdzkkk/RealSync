using System;

namespace RealSync.Core.Entities;

/// <summary>
/// Chi tiết mỗi lượt thử đăng bài (attempt) của một Publication Job.
/// </summary>
public class PublicationAttempt : BaseEntity
{
    public Guid PublicationJobId { get; set; }
    public PublicationJob PublicationJob { get; set; } = null!;

    public int AttemptNumber { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public long? DurationMs { get; set; }
    public int? ProviderHttpStatus { get; set; }
    public string? ProviderErrorCode { get; set; }
    public string? NormalizedErrorCategory { get; set; }
    public string? ProviderRequestId { get; set; }
    public string? RequestMetadataJson { get; set; }
    public string? ResponseMetadataJson { get; set; }
    public bool IsSuccess { get; set; }
    public string? RetryDecision { get; set; }
}
