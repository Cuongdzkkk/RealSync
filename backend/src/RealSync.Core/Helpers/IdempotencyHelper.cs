using System;
using System.Security.Cryptography;
using System.Text;

namespace RealSync.Core.Helpers;

public static class IdempotencyHelper
{
    /// <summary>
    /// Tạo một khóa duy nhất không trùng lặp (Idempotency Key) bằng SHA256.
    /// </summary>
    public static string GenerateKey(Guid workspaceId, Guid postId, Guid contentVariantId, Guid connectedAccountId, DateTime? scheduledAtBucket, string? mediaManifest)
    {
        var bucketStr = scheduledAtBucket?.ToString("yyyy-MM-dd_HH-mm") ?? "immediate";
        var mediaHash = mediaManifest ?? "no-media";
        var rawData = $"{workspaceId}:{postId}:{contentVariantId}:{connectedAccountId}:{bucketStr}:{mediaHash}";
        
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes).ToLower();
    }
}
