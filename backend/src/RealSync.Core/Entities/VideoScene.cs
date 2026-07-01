using System;
using RealSync.Shared.Enums;

namespace RealSync.Core.Entities;

/// <summary>
/// Phân cảnh (scene) trong storyboard của kịch bản video.
/// </summary>
public class VideoScene : BaseEntity
{
    public Guid VideoProjectId { get; set; }
    public VideoProject VideoProject { get; set; } = null!;

    public int Sequence { get; set; }
    public int DurationSeconds { get; set; }
    public string Narration { get; set; } = string.Empty;
    public string OnScreenText { get; set; } = string.Empty;
    
    public string VisualPrompt { get; set; } = string.Empty;
    public string? NegativePrompt { get; set; }
    public string? CameraDirection { get; set; }
    public string? ReferenceAssetIdsJson { get; set; }

    public VideoSceneStatus Status { get; set; } = VideoSceneStatus.Pending;
    
    public Guid? GeneratedAssetId { get; set; }
    public StoredFile? GeneratedAsset { get; set; }
}
