using System;
using RealSync.Core.Enums;

namespace RealSync.Core.Entities;

/// <summary>
/// Lưu trữ tokens OAuth đã được mã hóa tương ứng với ConnectedAccount.
/// </summary>
public class OAuthCredential : BaseEntity
{
    public Guid ConnectedAccountId { get; set; }
    public ConnectedAccount ConnectedAccount { get; set; } = null!;

    public string? SecretReference { get; set; }
    public string? EncryptionKeyVersion { get; set; }
    public string? AccessTokenEncrypted { get; set; }
    public string? RefreshTokenEncrypted { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? RefreshExpiresAt { get; set; }
    public CredentialStatus CredentialStatus { get; set; } = CredentialStatus.PendingSetup;
    public DateTime? LastRefreshAt { get; set; }
    public string? LastRefreshError { get; set; }
    public DateTime? RevokedAt { get; set; }
}
