using System;

namespace RealSync.Core.Entities;

/// <summary>
/// Thống kê sử dụng tài nguyên của provider hàng ngày.
/// </summary>
public class ProviderUsageDaily : BaseEntity
{
    public Guid ProviderCredentialId { get; set; }
    public ProviderCredential ProviderCredential { get; set; } = null!;

    public DateTime UsageDate { get; set; }
    public int RequestCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public long InputTokens { get; set; }
    public long OutputTokens { get; set; }
    public long GeneratedVideoSeconds { get; set; }
    public decimal EstimatedCost { get; set; }
    public int RateLimitCount { get; set; }
}
