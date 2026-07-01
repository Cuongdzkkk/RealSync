namespace RealSync.Core.Enums;

/// <summary>
/// Trạng thái của chứng thư / liên kết tài khoản.
/// </summary>
public enum CredentialStatus
{
    PendingSetup = 0,
    Active = 1,
    Expiring = 2,
    Expired = 3,
    Revoked = 4,
    Invalid = 5,
    Disabled = 6
}
