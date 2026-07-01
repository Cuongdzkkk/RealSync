using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RealSync.Core.Entities;
using RealSync.Core.Enums;
using RealSync.Core.Interfaces.Publishing;
using RealSync.Core.Models.Publishing;
using RealSync.Data.Context;
using RealSync.Services.Publishing;
using RealSync.Shared.DTOs.Requests.Publishing;

namespace RealSync.UnitTests.Publishing;

[TestFixture]
public class PublicationOrchestratorTests
{
    private RealSyncDbContext _context = null!;
    private Mock<IConnectorResolver> _resolverMock = null!;
    private Mock<IBackgroundJobClient> _backgroundJobClientMock = null!;
    private Mock<IPublishingConnector> _connectorMock = null!;
    private PublicationOrchestrator _orchestrator = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<RealSyncDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new RealSyncDbContext(options);
        _resolverMock = new Mock<IConnectorResolver>();
        _backgroundJobClientMock = new Mock<IBackgroundJobClient>();
        _connectorMock = new Mock<IPublishingConnector>();

        _orchestrator = new PublicationOrchestrator(_context, _resolverMock.Object, _backgroundJobClientMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task QueueAsync_WhenValidInput_ShouldSaveJobAndEnqueueBackgroundWorker()
    {
        var postId = Guid.NewGuid();
        var variant = new ContentVariant
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            ChannelType = PublishingChannelType.Website,
            Title = "Showcase",
            Caption = "Content"
        };
        _context.ContentVariants.Add(variant);
        await _context.SaveChangesAsync();

        var request = new QueuePublicationRequest(postId, variant.Id, null);

        var result = await _orchestrator.QueueAsync(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Status.Should().Be(PublicationJobStatus.Pending);

        var dbJob = await _context.PublicationJobs.FirstOrDefaultAsync(j => j.Id == result.Id);
        dbJob.Should().NotBeNull();
        dbJob!.PostId.Should().Be(postId);

        // Verify Hangfire enqueued execution
        _backgroundJobClientMock.Verify(
            x => x.Create(
                It.Is<Job>(j => j.Method.Name == "ExecuteAsync" && j.Args[0].ToString() == dbJob.Id.ToString()),
                It.IsAny<EnqueuedState>()),
            Times.Once);
    }

    [Test]
    public async Task QueueAsync_WhenCalledTwiceWithSamePayload_ShouldReturnExistingJobDueToIdempotency()
    {
        var postId = Guid.NewGuid();
        var variant = new ContentVariant
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            ChannelType = PublishingChannelType.Website,
            Title = "Showcase",
            Caption = "Content"
        };
        _context.ContentVariants.Add(variant);
        await _context.SaveChangesAsync();

        var request = new QueuePublicationRequest(postId, variant.Id, null);

        var result1 = await _orchestrator.QueueAsync(request, CancellationToken.None);
        var result2 = await _orchestrator.QueueAsync(request, CancellationToken.None);

        result2.Id.Should().Be(result1.Id);
        var jobsCount = await _context.PublicationJobs.CountAsync();
        jobsCount.Should().Be(1);
    }

    [Test]
    public async Task ExecuteAsync_WhenConnectorSucceeds_ShouldUpdateJobToPublishedAndSaveAttempt()
    {
        var postId = Guid.NewGuid();
        var post = new Post
        {
            Id = postId,
            Title = "Showcase",
            Content = "Post content",
            Status = "Draft",
            AuthorId = Guid.NewGuid()
        };
        var variant = new ContentVariant
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            ChannelType = PublishingChannelType.Website,
            Title = "Showcase Title",
            Caption = "Caption content"
        };
        _context.Posts.Add(post);
        _context.ContentVariants.Add(variant);
        await _context.SaveChangesAsync();

        var job = new PublicationJob
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            ContentVariantId = variant.Id,
            Status = PublicationJobStatus.Pending
        };
        _context.PublicationJobs.Add(job);
        await _context.SaveChangesAsync();

        _resolverMock.Setup(r => r.Resolve(PublishingChannelType.Website)).Returns(_connectorMock.Object);
        _connectorMock.Setup(c => c.ValidateAsync(It.IsAny<PublicationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidationResult.Success());
        _connectorMock.Setup(c => c.PublishAsync(It.IsAny<PublicationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PublishInitiationResult.Success("ext-123", "/public-url"));

        await _orchestrator.ExecuteAsync(job.Id, CancellationToken.None);

        var dbJob = await _context.PublicationJobs.FindAsync(job.Id);
        dbJob!.Status.Should().Be(PublicationJobStatus.Published);
        dbJob.PublishedUrl.Should().Be("/public-url");

        var attempt = await _context.PublicationAttempts.FirstOrDefaultAsync(a => a.PublicationJobId == job.Id);
        attempt.Should().NotBeNull();
        attempt!.IsSuccess.Should().BeTrue();
        attempt.AttemptNumber.Should().Be(1);
    }

    [Test]
    public async Task ExecuteAsync_WhenValidationFails_ShouldMarkNeedsReviewAndNotTriggerPublish()
    {
        var postId = Guid.NewGuid();
        var post = new Post
        {
            Id = postId,
            Title = "Showcase",
            Content = "Post content",
            Status = "Draft",
            AuthorId = Guid.NewGuid()
        };
        var variant = new ContentVariant
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            ChannelType = PublishingChannelType.Website,
            Title = "Showcase Title",
            Caption = "Caption content"
        };
        _context.Posts.Add(post);
        _context.ContentVariants.Add(variant);
        await _context.SaveChangesAsync();

        var job = new PublicationJob
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            ContentVariantId = variant.Id,
            Status = PublicationJobStatus.Pending
        };
        _context.PublicationJobs.Add(job);
        await _context.SaveChangesAsync();

        _resolverMock.Setup(r => r.Resolve(PublishingChannelType.Website)).Returns(_connectorMock.Object);
        _connectorMock.Setup(c => c.ValidateAsync(It.IsAny<PublicationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidationResult.Failure("Tiêu đề trống"));

        await _orchestrator.ExecuteAsync(job.Id, CancellationToken.None);

        var dbJob = await _context.PublicationJobs.FindAsync(job.Id);
        dbJob!.Status.Should().Be(PublicationJobStatus.NeedsReview);
        dbJob.LastErrorMessage.Should().Contain("Tiêu đề trống");

        _connectorMock.Verify(c => c.PublishAsync(It.IsAny<PublicationContext>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
