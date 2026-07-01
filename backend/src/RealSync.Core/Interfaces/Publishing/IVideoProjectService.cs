using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RealSync.Core.Entities;

namespace RealSync.Core.Interfaces.Publishing;

public interface IVideoProjectService
{
    Task<VideoProject> CreateProjectAsync(Guid variantId, Guid? postId, CancellationToken cancellationToken);
    Task<VideoProject?> GetProjectByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<VideoProject> UpdateStoryboardAsync(Guid id, List<VideoSceneUpdateDto> scenes, CancellationToken cancellationToken);
    
    Task<VideoGenerationJob> StartGenerationAsync(Guid projectId, CancellationToken cancellationToken);
    Task<VideoGenerationJob> StartRenderingAsync(Guid projectId, CancellationToken cancellationToken);
    
    // Background Job workers called by Hangfire
    Task GenerateSceneBackgroundAsync(Guid sceneId, Guid jobId, CancellationToken cancellationToken);
    Task CheckSceneGenerationStatusBackgroundAsync(Guid sceneId, Guid jobId, CancellationToken cancellationToken);
    Task RenderProjectBackgroundAsync(Guid projectId, Guid jobId, CancellationToken cancellationToken);
}

public class VideoSceneUpdateDto
{
    public Guid Id { get; set; }
    public string Narration { get; set; } = string.Empty;
    public string OnScreenText { get; set; } = string.Empty;
    public string VisualPrompt { get; set; } = string.Empty;
    public string? NegativePrompt { get; set; }
    public string? CameraDirection { get; set; }
    public int DurationSeconds { get; set; }
}
