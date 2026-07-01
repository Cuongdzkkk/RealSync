using System;
using System.Collections.Generic;
using RealSync.Core.Enums;

namespace RealSync.Core.Entities;

/// <summary>
/// Job quản lý quá trình xuất bản bài viết lên một tài khoản liên kết.
/// </summary>
public class PublicationJob : BaseEntity
{
    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;

    public Guid ContentVariantId { get; set; }
    public ContentVariant ContentVariant { get; set; } = null!;

    public Guid? ConnectedAccountId { get; set; }
    public ConnectedAccount? ConnectedAccount { get; set; }

    public PublishMode PublishMode { get; set; } = PublishMode.Direct;
    public DateTime? ScheduledAtUtc { get; set; }
    public PublicationJobStatus Status { get; set; } = PublicationJobStatus.Pending;
    public int Priority { get; set; } = 0;
    public string IdempotencyKey { get; set; } = string.Empty;
    public string? PayloadSnapshotJson { get; set; }
    public string? MediaManifestJson { get; set; }
    public string? ExternalPostId { get; set; }
    public string? ExternalPublishId { get; set; }
    public string? PublishedUrl { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? RemoteStatus { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetryCount { get; set; } = 5;
    public DateTime? NextRetryAt { get; set; }
    public DateTime? LastAttemptAt { get; set; }
    public string? LastErrorCategory { get; set; }
    public string? LastErrorCode { get; set; }
    public string? LastErrorMessage { get; set; }
    public string? CorrelationId { get; set; }

    // Flag cho các bản ghi cũ migrated chưa được verified qua API
    public bool LegacyUnverified { get; set; } = false;

    // Navigation
    public ICollection<PublicationAttempt> PublicationAttempts { get; set; } = new List<PublicationAttempt>();
}
