using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RealSync.Core.Entities;
using RealSync.Core.Enums;
using RealSync.Core.Interfaces;
using RealSync.Core.Interfaces.Media;
using RealSync.Core.Interfaces.Publishing;
using RealSync.Data.Context;
using RealSync.Services.Implementations;
using RealSync.Services.Options;
using RealSync.Shared.DTOs.Requests;
using RealSync.Shared.Enums;
using RealSync.Shared.Exceptions;

namespace RealSync.Services.Publishing;

public class VideoProjectService : IVideoProjectService
{
    private readonly RealSyncDbContext _context;
    private readonly IVideoGenerationProvider _videoProvider;
    private readonly IVideoRenderService _videoRender;
    private readonly IFileStorageService _localStorage;
    private readonly R2FileStorageService _r2Storage;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly HttpClient _httpClient;
    private readonly VideoOptions _videoOptions;
    private readonly R2StorageOptions _r2Options;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<VideoProjectService> _logger;

    public VideoProjectService(
        RealSyncDbContext context,
        IVideoGenerationProvider videoProvider,
        IVideoRenderService videoRender,
        IFileStorageService localStorage,
        R2FileStorageService r2Storage,
        IBackgroundJobClient backgroundJobClient,
        HttpClient httpClient,
        IOptions<VideoOptions> videoOptions,
        IConfiguration configuration,
        IHostEnvironment environment,
        ILogger<VideoProjectService> logger)
    {
        _context = context;
        _videoProvider = videoProvider;
        _videoRender = videoRender;
        _localStorage = localStorage;
        _r2Storage = r2Storage;
        _backgroundJobClient = backgroundJobClient;
        _httpClient = httpClient;
        _videoOptions = videoOptions.Value;
        _r2Options = configuration.GetSection("R2Storage").Get<R2StorageOptions>() ?? new R2StorageOptions();
        _environment = environment;
        _logger = logger;
    }

    public async Task<VideoProject> CreateProjectAsync(Guid variantId, Guid? postId, CancellationToken cancellationToken)
    {
        Guid finalVariantId = variantId;

        if (finalVariantId == Guid.Empty)
        {
            if (!postId.HasValue || postId == Guid.Empty)
            {
                throw new BusinessException("Phải cung cấp ContentVariantId hoặc PostId để tạo dự án video.");
            }

            var existingVariant = await _context.ContentVariants
                .FirstOrDefaultAsync(v => v.PostId == postId.Value, cancellationToken);

            if (existingVariant == null)
            {
                var post = await _context.Posts.FindAsync(new object[] { postId.Value }, cancellationToken)
                    ?? throw new NotFoundException("Post", postId.Value);

                existingVariant = new ContentVariant
                {
                    PostId = postId.Value,
                    ChannelType = Core.Enums.PublishingChannelType.Website,
                    Title = post.Title,
                    Caption = post.Content ?? string.Empty,
                    Summary = post.Summary ?? string.Empty,
                    CallToAction = "Liên hệ ngay"
                };
                _context.ContentVariants.Add(existingVariant);
                await _context.SaveChangesAsync(cancellationToken);
            }

            finalVariantId = existingVariant.Id;
        }

        var variant = await _context.ContentVariants
            .Include(v => v.Post)
            .FirstOrDefaultAsync(v => v.Id == finalVariantId, cancellationToken)
            ?? throw new NotFoundException("ContentVariant", finalVariantId);

        var title = variant.Title ?? variant.Post.Title;

        var project = new VideoProject
        {
            PostId = variant.PostId,
            ContentVariantId = finalVariantId,
            Title = $"Video - {title}",
            TargetChannel = variant.ChannelType.ToString(),
            AspectRatio = "9:16",
            TargetDurationSeconds = 30,
            Status = VideoProjectStatus.Draft
        };

        // Create 3 default scenes
        project.Scenes.Add(new VideoScene
        {
            Sequence = 1,
            DurationSeconds = 8,
            OnScreenText = title,
            Narration = $"Chào mừng quý vị đến với RealSync. Hôm nay chúng tôi xin giới thiệu một bất động sản nổi bật.",
            VisualPrompt = "Luxury modern villa exterior at sunset, warm cinematic lighting, high-end architecture, 8k resolution, vertical 9:16 aspect ratio",
            CameraDirection = "Zoom in slow",
            Status = VideoSceneStatus.Pending
        });

        project.Scenes.Add(new VideoScene
        {
            Sequence = 2,
            DurationSeconds = 12,
            OnScreenText = variant.Summary ?? "Thông tin dự án và giá bán hấp dẫn",
            Narration = $"Bất động sản có thiết kế sang trọng, tối ưu công năng, tiện nghi đầy đủ và giao thông vô cùng thuận tiện.",
            VisualPrompt = "Spacious modern home interior living room, elegant furniture, minimalist design, soft natural light, vertical 9:16 aspect ratio",
            CameraDirection = "Pan left slow",
            Status = VideoSceneStatus.Pending
        });

        project.Scenes.Add(new VideoScene
        {
            Sequence = 3,
            DurationSeconds = 10,
            OnScreenText = variant.CallToAction ?? "Liên hệ Hotline ngay hôm nay",
            Narration = $"Hãy liên hệ ngay hôm nay để nhận thông tin chi tiết và đặt lịch xem nhà trực tiếp.",
            VisualPrompt = "Broker greeting client at modern office, professional and welcoming atmosphere, business card overlay, vertical 9:16 aspect ratio",
            CameraDirection = "Stationary",
            Status = VideoSceneStatus.Pending
        });

        _context.VideoProjects.Add(project);
        await _context.SaveChangesAsync(cancellationToken);

        return project;
    }

    public async Task<VideoProject?> GetProjectByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.VideoProjects
            .Include(p => p.Scenes)
            .Include(p => p.FinalAsset)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<VideoProject> UpdateStoryboardAsync(Guid id, List<VideoSceneUpdateDto> scenes, CancellationToken cancellationToken)
    {
        var project = await _context.VideoProjects
            .Include(p => p.Scenes)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException("VideoProject", id);

        foreach (var sceneDto in scenes)
        {
            var scene = project.Scenes.FirstOrDefault(s => s.Id == sceneDto.Id);
            if (scene != null)
            {
                scene.Narration = sceneDto.Narration;
                scene.OnScreenText = sceneDto.OnScreenText;
                scene.VisualPrompt = sceneDto.VisualPrompt;
                scene.NegativePrompt = sceneDto.NegativePrompt;
                scene.CameraDirection = sceneDto.CameraDirection;
                scene.DurationSeconds = sceneDto.DurationSeconds;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return project;
    }

    public async Task<VideoGenerationJob> StartGenerationAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var project = await _context.VideoProjects
            .Include(p => p.Scenes)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken)
            ?? throw new NotFoundException("VideoProject", projectId);

        project.Status = VideoProjectStatus.GeneratingScenes;

        var parentJob = new VideoGenerationJob
        {
            VideoProjectId = projectId,
            Provider = "Veo",
            Model = _videoOptions.Model,
            Status = VideoJobStatus.Processing,
            StartedAt = DateTime.UtcNow
        };
        _context.VideoGenerationJobs.Add(parentJob);

        foreach (var scene in project.Scenes)
        {
            scene.Status = VideoSceneStatus.Generating;
            
            var job = new VideoGenerationJob
            {
                VideoProjectId = projectId,
                VideoSceneId = scene.Id,
                Provider = "Veo",
                Model = _videoOptions.Model,
                Status = VideoJobStatus.Pending
            };
            _context.VideoGenerationJobs.Add(job);
            await _context.SaveChangesAsync(cancellationToken);

            // Enqueue Hangfire background worker for each scene
            _backgroundJobClient.Enqueue<IVideoProjectService>(s => 
                s.GenerateSceneBackgroundAsync(scene.Id, job.Id, CancellationToken.None));
        }

        await _context.SaveChangesAsync(cancellationToken);
        return parentJob;
    }

    public async Task<VideoGenerationJob> StartRenderingAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var project = await _context.VideoProjects
            .Include(p => p.Scenes)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken)
            ?? throw new NotFoundException("VideoProject", projectId);

        if (project.Scenes.Any(s => s.Status != VideoSceneStatus.Completed))
        {
            throw new BusinessException("Tất cả các phân cảnh phải hoàn tất sinh video trước khi render.");
        }

        project.Status = VideoProjectStatus.Rendering;

        var job = new VideoGenerationJob
        {
            VideoProjectId = projectId,
            Provider = "FFmpeg",
            Model = "Stitcher",
            Status = VideoJobStatus.Pending
        };
        _context.VideoGenerationJobs.Add(job);
        await _context.SaveChangesAsync(cancellationToken);

        // Enqueue Hangfire background worker for stitching
        _backgroundJobClient.Enqueue<IVideoProjectService>(s => 
            s.RenderProjectBackgroundAsync(projectId, job.Id, CancellationToken.None));

        return job;
    }

    public async Task GenerateSceneBackgroundAsync(Guid sceneId, Guid jobId, CancellationToken cancellationToken)
    {
        var scene = await _context.VideoScenes
            .Include(s => s.VideoProject)
            .FirstOrDefaultAsync(s => s.Id == sceneId, cancellationToken);
        var job = await _context.VideoGenerationJobs.FindAsync(new object[] { jobId }, cancellationToken);

        if (scene == null || job == null) return;

        job.Status = VideoJobStatus.Processing;
        job.StartedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            var res = await _videoProvider.StartGenerationAsync(
                scene.VisualPrompt, scene.VideoProject.AspectRatio, scene.DurationSeconds, cancellationToken);

            job.OperationId = res.OperationId;

            if (res.Done)
            {
                if (string.IsNullOrEmpty(res.VideoUrl))
                {
                    throw new BusinessException("Google Veo không trả về URL của video.");
                }
                await DownloadAndAttachSceneVideoAsync(scene, job, res.VideoUrl, cancellationToken);
            }
            else
            {
                await _context.SaveChangesAsync(cancellationToken);
                // Schedule status check in 5 seconds
                _backgroundJobClient.Schedule<IVideoProjectService>(s => 
                    s.CheckSceneGenerationStatusBackgroundAsync(sceneId, jobId, CancellationToken.None), 
                    TimeSpan.FromSeconds(5));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating scene video.");
            job.Status = VideoJobStatus.Failed;
            job.ErrorMessage = ex.Message;
            scene.Status = VideoSceneStatus.Failed;
            scene.VideoProject.Status = VideoProjectStatus.Failed;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task CheckSceneGenerationStatusBackgroundAsync(Guid sceneId, Guid jobId, CancellationToken cancellationToken)
    {
        var scene = await _context.VideoScenes
            .Include(s => s.VideoProject)
            .FirstOrDefaultAsync(s => s.Id == sceneId, cancellationToken);
        var job = await _context.VideoGenerationJobs.FindAsync(new object[] { jobId }, cancellationToken);

        if (scene == null || job == null || string.IsNullOrEmpty(job.OperationId)) return;

        job.RetryCount++;
        if (job.RetryCount > 30) // timeout after 30 checks * 5s = 150 seconds
        {
            job.Status = VideoJobStatus.Failed;
            job.ErrorMessage = "Quá thời gian chờ xử lý video từ Google Veo.";
            scene.Status = VideoSceneStatus.Failed;
            scene.VideoProject.Status = VideoProjectStatus.Failed;
            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        try
        {
            var res = await _videoProvider.GetOperationStatusAsync(job.OperationId, cancellationToken);
            job.ProgressPercent = res.ProgressPercent;

            if (res.Done)
            {
                if (!string.IsNullOrEmpty(res.ErrorMessage))
                {
                    throw new BusinessException(res.ErrorMessage);
                }

                if (string.IsNullOrEmpty(res.VideoUrl))
                {
                    throw new BusinessException("Trạng thái hoàn thành nhưng không tìm thấy Video URL.");
                }

                await DownloadAndAttachSceneVideoAsync(scene, job, res.VideoUrl, cancellationToken);
            }
            else
            {
                await _context.SaveChangesAsync(cancellationToken);
                // Reschedule status check in 5 seconds
                _backgroundJobClient.Schedule<IVideoProjectService>(s => 
                    s.CheckSceneGenerationStatusBackgroundAsync(sceneId, jobId, CancellationToken.None), 
                    TimeSpan.FromSeconds(5));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking scene generation status.");
            job.Status = VideoJobStatus.Failed;
            job.ErrorMessage = ex.Message;
            scene.Status = VideoSceneStatus.Failed;
            scene.VideoProject.Status = VideoProjectStatus.Failed;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RenderProjectBackgroundAsync(Guid projectId, Guid jobId, CancellationToken cancellationToken)
    {
        var project = await _context.VideoProjects
            .Include(p => p.Scenes)
            .ThenInclude(s => s.GeneratedAsset)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);
        var job = await _context.VideoGenerationJobs.FindAsync(new object[] { jobId }, cancellationToken);

        if (project == null || job == null) return;

        job.Status = VideoJobStatus.Processing;
        job.StartedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        var tempDirectory = Path.Combine(_videoOptions.TempDirectory, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        var downloadedFiles = new List<string>();

        try
        {
            // 1. Download all scene clips locally
            var scenesOrdered = project.Scenes.OrderBy(s => s.Sequence).ToList();
            var overlays = new List<SceneOverlayText>();
            var currentTime = 0;

            foreach (var scene in scenesOrdered)
            {
                if (scene.GeneratedAsset == null)
                {
                    throw new BusinessException($"Phân cảnh {scene.Sequence} thiếu file video đã sinh.");
                }

                var tempScenePath = Path.Combine(tempDirectory, $"scene_{scene.Sequence}.mp4");
                
                // Read from Storage and write locally
                using (var readStream = await _localStorage.OpenReadAsync(scene.GeneratedAsset.RelativePath, cancellationToken))
                using (var writeStream = new FileStream(tempScenePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await readStream.CopyToAsync(writeStream, cancellationToken);
                }

                downloadedFiles.Add(tempScenePath);

                if (!string.IsNullOrEmpty(scene.OnScreenText))
                {
                    overlays.Add(new SceneOverlayText
                    {
                        StartTimeSeconds = currentTime,
                        DurationSeconds = scene.DurationSeconds,
                        Text = scene.OnScreenText
                    });
                }

                currentTime += scene.DurationSeconds;
            }

            // 2. Call render service
            var finalTempPath = await _videoRender.ConcatenateAndOverlayAsync(
                downloadedFiles, overlays, tempDirectory, cancellationToken);

            // 3. Save final render result to storage
            StoredFileResult fileResult;
            using (var finalStream = new FileStream(finalTempPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (_r2Options.IsConfigured)
                {
                    // Upload to R2
                    var key = $"workspaces/default/video-projects/{projectId}/render_{Guid.NewGuid():N}.mp4";
                    var uploadResult = await _r2Storage.UploadAsync(key, new FileUploadRequest
                    {
                        FileName = Path.GetFileName(finalTempPath),
                        ContentType = "video/mp4",
                        Length = finalStream.Length,
                        OpenReadStream = () => new FileStream(finalTempPath, FileMode.Open, FileAccess.Read, FileShare.Read)
                    }, cancellationToken);

                    // Add metadata record in database
                    var storedFile = new StoredFile
                    {
                        OriginalFileName = $"render_{project.Id:N}.mp4",
                        StoredFileName = Path.GetFileName(finalTempPath),
                        RelativePath = key,
                        ContentType = "video/mp4",
                        Extension = ".mp4",
                        SizeBytes = finalStream.Length,
                        IsPublic = true,
                        EntityType = "VideoProject",
                        EntityId = projectId,
                        CreatedAt = DateTimeOffset.UtcNow
                    };
                    _context.StoredFiles.Add(storedFile);
                    await _context.SaveChangesAsync(cancellationToken);

                    fileResult = new StoredFileResult
                    {
                        Id = storedFile.Id,
                        RelativePath = key,
                        Url = uploadResult.Url,
                        OriginalFileName = storedFile.OriginalFileName,
                        StoredFileName = storedFile.StoredFileName,
                        ContentType = storedFile.ContentType,
                        SizeBytes = storedFile.SizeBytes
                    };
                }
                else
                {
                    // Save locally
                    fileResult = await _localStorage.SavePublicImageAsync(
                        finalStream, $"render_{project.Id:N}.mp4", "video/mp4", "VideoProject", cancellationToken);
                }
            }

            // 4. Update entities status
            project.Status = VideoProjectStatus.Completed;
            project.FinalAssetId = fileResult.Id;

            job.Status = VideoJobStatus.Completed;
            job.CompletedAt = DateTime.UtcNow;
            job.OutputAssetId = fileResult.Id;

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering final video project.");
            job.Status = VideoJobStatus.Failed;
            job.ErrorMessage = ex.Message;
            project.Status = VideoProjectStatus.Failed;
            await _context.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            // Cleanup local temp files
            if (Directory.Exists(tempDirectory))
            {
                try { Directory.Delete(tempDirectory, recursive: true); } catch { /* ignore */ }
            }
        }
    }

    private async Task DownloadAndAttachSceneVideoAsync(VideoScene scene, VideoGenerationJob job, string videoUrl, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Downloading video clip from: {Url}", videoUrl);

        using var response = await _httpClient.GetAsync(videoUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var downloadStream = await response.Content.ReadAsStreamAsync(cancellationToken);

        StoredFileResult fileResult;
        
        if (_r2Options.IsConfigured)
        {
            // 1. Upload to R2
            var key = $"workspaces/default/video-projects/{scene.VideoProjectId}/scenes/scene_{scene.Sequence}_{Guid.NewGuid():N}.mp4";
            var tempFile = Path.Combine(_videoOptions.TempDirectory, $"{Guid.NewGuid():N}.mp4");
            Directory.CreateDirectory(_videoOptions.TempDirectory);

            try
            {
                using (var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await downloadStream.CopyToAsync(fileStream, cancellationToken);
                }

                var uploadResult = await _r2Storage.UploadAsync(key, new FileUploadRequest
                {
                    FileName = $"scene_{scene.Sequence}.mp4",
                    ContentType = "video/mp4",
                    Length = new FileInfo(tempFile).Length,
                    OpenReadStream = () => new FileStream(tempFile, FileMode.Open, FileAccess.Read, FileShare.Read)
                }, cancellationToken);

                // Add metadata record in database
                var storedFile = new StoredFile
                {
                    OriginalFileName = $"scene_{scene.Sequence}.mp4",
                    StoredFileName = Path.GetFileName(tempFile),
                    RelativePath = key,
                    ContentType = "video/mp4",
                    Extension = ".mp4",
                    SizeBytes = new FileInfo(tempFile).Length,
                    IsPublic = true,
                    EntityType = "VideoScene",
                    EntityId = scene.Id,
                    CreatedAt = DateTimeOffset.UtcNow
                };
                _context.StoredFiles.Add(storedFile);
                await _context.SaveChangesAsync(cancellationToken);

                fileResult = new StoredFileResult
                {
                    Id = storedFile.Id,
                    RelativePath = key,
                    Url = uploadResult.Url,
                    OriginalFileName = storedFile.OriginalFileName,
                    StoredFileName = storedFile.StoredFileName,
                    ContentType = storedFile.ContentType,
                    SizeBytes = storedFile.SizeBytes
                };
            }
            finally
            {
                if (File.Exists(tempFile)) { try { File.Delete(tempFile); } catch {} }
            }
        }
        else
        {
            // 2. Save locally
            fileResult = await _localStorage.SavePublicImageAsync(
                downloadStream, $"scene_{scene.Sequence}.mp4", "video/mp4", "VideoScene", cancellationToken);
        }

        // 3. Update scene and generation job
        scene.Status = VideoSceneStatus.Completed;
        scene.GeneratedAssetId = fileResult.Id;

        job.Status = VideoJobStatus.Completed;
        job.CompletedAt = DateTime.UtcNow;
        job.OutputAssetId = fileResult.Id;

        // Check if all scenes are done in the project
        var project = await _context.VideoProjects
            .Include(p => p.Scenes)
            .FirstOrDefaultAsync(p => p.Id == scene.VideoProjectId, cancellationToken);

        if (project != null)
        {
            if (project.Scenes.All(s => s.Status == VideoSceneStatus.Completed))
            {
                project.Status = VideoProjectStatus.StoryboardGenerated;
                
                // Track usage budget statistics
                var usageDate = DateTime.UtcNow.Date;
                var usage = await _context.ProviderUsageDailies
                    .FirstOrDefaultAsync(u => u.UsageDate == usageDate, cancellationToken);

                if (usage == null)
                {
                    // Find default provider credential if any
                    var cred = await _context.ProviderCredentials
                        .FirstOrDefaultAsync(c => c.Provider == "Veo", cancellationToken);
                    
                    usage = new ProviderUsageDaily
                    {
                        ProviderCredentialId = cred?.Id ?? Guid.Empty,
                        UsageDate = usageDate,
                        RequestCount = 0,
                        SuccessCount = 0,
                        EstimatedCost = 0
                    };
                    _context.ProviderUsageDailies.Add(usage);
                }

                usage.RequestCount += project.Scenes.Count;
                usage.SuccessCount += project.Scenes.Count;
                usage.GeneratedVideoSeconds += project.Scenes.Sum(s => s.DurationSeconds);
                // Veo estimated cost: approx $0.07 per video scene (average 10s)
                usage.EstimatedCost += project.Scenes.Count * 0.07m;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
