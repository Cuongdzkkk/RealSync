using System;
using System.Collections.Generic;
using RealSync.Core.Enums;

namespace RealSync.Core.Entities;

/// <summary>
/// Tài khoản liên kết trên các nền tảng xã hội/portal.
/// </summary>
public class ConnectedAccount : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public string Provider { get; set; } = string.Empty; // e.g. "Facebook", "TikTok", "Zalo", "YouTube", "Website"
    public PublishingChannelType ChannelType { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string ExternalAccountId { get; set; } = string.Empty;
    public string? ExternalParentAccountId { get; set; }
    public string? ProfileUrl { get; set; }
    public string? AvatarUrl { get; set; }
    public CredentialStatus Status { get; set; } = CredentialStatus.PendingSetup;
    public string? GrantedScopesJson { get; set; }
    public DateTime? TokenExpiresAt { get; set; }
    public DateTime? LastValidatedAt { get; set; }
    public string? LastErrorCode { get; set; }
    public string? LastErrorMessage { get; set; }

    // Navigation properties
    public OAuthCredential? OAuthCredential { get; set; }
    public ICollection<PublicationJob> PublicationJobs { get; set; } = new List<PublicationJob>();
}
