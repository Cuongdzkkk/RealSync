using System;
using RealSync.Shared.Enums;

namespace RealSync.Core.Entities;

/// <summary>
/// Tiến trình sinh video phân cảnh (Veo) hoặc ghép nối (FFmpeg).
/// </summary>
public class VideoGenerationJob : BaseEntity
{
    public Guid VideoProjectId { get; set; }
    public VideoProject VideoProject { get; set; } = null!;

    public Guid? VideoSceneId { get; set; }
    public VideoScene? VideoScene { get; set; }

    public string Provider { get; set; } = "Veo";
    public string Model { get; set; } = "veo-3.1-generate-preview";
    public string? OperationId { get; set; }
    
    public VideoJobStatus Status { get; set; } = VideoJobStatus.Pending;
    public int? ProgressPercent { get; set; }
    
    public Guid? OutputAssetId { get; set; }
    public StoredFile? OutputAsset { get; set; }

    public string? ErrorCategory { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }

    public int RetryCount { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
