using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RealSync.Core.Entities;
using RealSync.Core.Interfaces;
using RealSync.Core.Interfaces.Media;
using RealSync.Core.Interfaces.Publishing;
using RealSync.Data.Context;
using RealSync.Services.Options;
using RealSync.Services.Publishing;
using RealSync.Services.Implementations;
using RealSync.Shared.Enums;
using RealSync.Shared.Exceptions;

namespace RealSync.UnitTests.Publishing;

[TestFixture]
public class VideoProjectServiceTests
{
    private RealSyncDbContext _context = null!;
    private Mock<IVideoGenerationProvider> _videoProviderMock = null!;
    private Mock<IVideoRenderService> _videoRenderMock = null!;
    private Mock<IFileStorageService> _localStorageMock = null!;
    private Mock<R2FileStorageService> _r2StorageMock = null!;
    private Mock<IBackgroundJobClient> _backgroundJobClientMock = null!;
    private Mock<IHostEnvironment> _environmentMock = null!;
    private Mock<ILogger<VideoProjectService>> _loggerMock = null!;
    private Mock<IConfiguration> _configurationMock = null!;
    private HttpClient _httpClient = null!;

    private VideoOptions _videoOptions = null!;
    private VideoProjectService _service = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<RealSyncDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new RealSyncDbContext(options);
        _videoProviderMock = new Mock<IVideoGenerationProvider>();
        _videoRenderMock = new Mock<IVideoRenderService>();
        _localStorageMock = new Mock<IFileStorageService>();
        _backgroundJobClientMock = new Mock<IBackgroundJobClient>();
        _environmentMock = new Mock<IHostEnvironment>();
        _loggerMock = new Mock<ILogger<VideoProjectService>>();
        
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(c => c.GetSection("R2Storage")).Returns(new Mock<IConfigurationSection>().Object);

        _r2StorageMock = new Mock<R2FileStorageService>(_configurationMock.Object);

        _httpClient = new HttpClient();

        _videoOptions = new VideoOptions
        {
            Provider = "Veo",
            Model = "veo-3.1-generate-preview",
            FfmpegPath = "ffmpeg",
            FfprobePath = "ffprobe",
            TempDirectory = Path.GetTempPath(),
            DailyBudget = 10.0m
        };

        _environmentMock.Setup(e => e.EnvironmentName).Returns("Development");

        _service = new VideoProjectService(
            _context,
            _videoProviderMock.Object,
            _videoRenderMock.Object,
            _localStorageMock.Object,
            _r2StorageMock.Object,
            _backgroundJobClientMock.Object,
            _httpClient,
            Options.Create(_videoOptions),
            _configurationMock.Object,
            _environmentMock.Object,
            _loggerMock.Object
        );
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        _httpClient.Dispose();
    }

    [Test]
    public async Task CreateProjectAsync_ShouldInitializeProjectAndThreeScenes()
    {
        // Arrange
        var post = new Post
        {
            Id = Guid.NewGuid(),
            Title = "Căn hộ Quận 2",
            AuthorId = Guid.NewGuid()
        };
        _context.Posts.Add(post);

        var variant = new ContentVariant
        {
            Id = Guid.NewGuid(),
            PostId = post.Id,
            ChannelType = RealSync.Core.Enums.PublishingChannelType.TikTok,
            Title = "Căn hộ Quận 2 bán nhanh",
            Summary = "Đầy đủ tiện ích, gần sông Sài Gòn",
            CallToAction = "Liên hệ 0909"
        };
        _context.ContentVariants.Add(variant);
        await _context.SaveChangesAsync();

        // Act
        var project = await _service.CreateProjectAsync(variant.Id, null, CancellationToken.None);

        // Assert
        project.Should().NotBeNull();
        project.PostId.Should().Be(post.Id);
        project.ContentVariantId.Should().Be(variant.Id);
        project.Title.Should().Contain("Căn hộ Quận 2 bán nhanh");
        project.Status.Should().Be(VideoProjectStatus.Draft);
        project.Scenes.Should().HaveCount(3);
        project.Scenes.OrderBy(s => s.Sequence).First().VisualPrompt.Should().Contain("Luxury modern");
    }

    [Test]
    public async Task StartGenerationAsync_ShouldTransitionStatusAndEnqueueJobs()
    {
        // Arrange
        var post = new Post
        {
            Id = Guid.NewGuid(),
            Title = "Căn hộ Quận 2",
            AuthorId = Guid.NewGuid()
        };
        _context.Posts.Add(post);

        var variant = new ContentVariant
        {
            Id = Guid.NewGuid(),
            PostId = post.Id,
            ChannelType = RealSync.Core.Enums.PublishingChannelType.TikTok,
            Title = "Căn hộ Quận 2 bán nhanh"
        };
        _context.ContentVariants.Add(variant);

        var project = new VideoProject
        {
            PostId = post.Id,
            ContentVariantId = variant.Id,
            Title = "Video Dự án",
            Status = VideoProjectStatus.Draft
        };
        project.Scenes.Add(new VideoScene
        {
            Sequence = 1,
            VisualPrompt = "Scene 1 Prompt",
            Status = VideoSceneStatus.Pending
        });
        _context.VideoProjects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        var resultJob = await _service.StartGenerationAsync(project.Id, CancellationToken.None);

        // Assert
        resultJob.Should().NotBeNull();
        project.Status.Should().Be(VideoProjectStatus.GeneratingScenes);
        project.Scenes.First().Status.Should().Be(VideoSceneStatus.Generating);
        
        _backgroundJobClientMock.Verify(b => b.Create(
            It.Is<Job>(j => j.Method.Name == "GenerateSceneBackgroundAsync"),
            It.IsAny<IState>()), Times.Once);
    }

    [Test]
    public void StartRenderingAsync_ShouldThrowException_WhenSomeScenesAreNotCompleted()
    {
        // Arrange
        var project = new VideoProject
        {
            PostId = Guid.NewGuid(),
            ContentVariantId = Guid.NewGuid(),
            Title = "Video Dự án",
            Status = VideoProjectStatus.GeneratingScenes
        };
        project.Scenes.Add(new VideoScene
        {
            Sequence = 1,
            VisualPrompt = "Scene 1 Prompt",
            Status = VideoSceneStatus.Generating // Not completed
        });
        _context.VideoProjects.Add(project);
        _context.SaveChanges();

        // Act & Assert
        Assert.ThrowsAsync<BusinessException>(async () =>
        {
            await _service.StartRenderingAsync(project.Id, CancellationToken.None);
        });
    }
}
