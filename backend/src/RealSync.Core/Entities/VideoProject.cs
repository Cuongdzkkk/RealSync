using System;
using System.Collections.Generic;
using RealSync.Shared.Enums;

namespace RealSync.Core.Entities;

/// <summary>
/// Dự án dựng video marketing BĐS.
/// </summary>
public class VideoProject : BaseEntity
{
    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;

    public Guid ContentVariantId { get; set; }
    public ContentVariant ContentVariant { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string TargetChannel { get; set; } = "TikTok";
    public string AspectRatio { get; set; } = "9:16";
    public int TargetDurationSeconds { get; set; } = 30;
    
    public VideoProjectStatus Status { get; set; } = VideoProjectStatus.Draft;
    public int ApprovedStoryboardVersion { get; set; } = 1;
    
    public Guid? FinalAssetId { get; set; }
    public StoredFile? FinalAsset { get; set; }

    public ICollection<VideoScene> Scenes { get; set; } = new List<VideoScene>();
}
