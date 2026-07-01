using System;
using System.Collections.Generic;
using RealSync.Core.Enums;

namespace RealSync.Core.Entities;

/// <summary>
/// Lưu cấu hình provider/API key dùng chung (ví dụ Gemini, OpenAI, Veo...)
/// </summary>
public class ProviderCredential : BaseEntity
{
    public Guid? WorkspaceId { get; set; }
    public string Provider { get; set; } = string.Empty; // Gemini, OpenAI, Veo, Zalo OA...
    public string CredentialType { get; set; } = string.Empty; // ApiKey, OAuthClient...
    public string? SecretReference { get; set; }
    public CredentialStatus Status { get; set; } = CredentialStatus.PendingSetup;
    public int Priority { get; set; } = 0;
    public string? AllowedCapabilitiesJson { get; set; }
    public decimal? BudgetDaily { get; set; }
    public decimal? BudgetMonthly { get; set; }
    public string? QuotaMetadataJson { get; set; }
    public DateTime? LastHealthCheckAt { get; set; }
    public DateTime? LastSuccessAt { get; set; }
    public DateTime? LastFailureAt { get; set; }
    public int FailureCount { get; set; }
    public string? DisabledReason { get; set; }

    public ICollection<ProviderUsageDaily> ProviderUsageDailies { get; set; } = new List<ProviderUsageDaily>();
}
