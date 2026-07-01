namespace RealSync.Shared.Enums;

/// <summary>
/// Trạng thái của dự án dựng video.
/// </summary>
public enum VideoProjectStatus
{
    Draft = 0,
    StoryboardGenerating = 1,
    StoryboardGenerated = 2,
    GeneratingScenes = 3,
    Rendering = 4,
    Completed = 5,
    Failed = 6
}
