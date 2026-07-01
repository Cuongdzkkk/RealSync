using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using RealSync.Core.Entities;
using RealSync.Core.Enums;
using RealSync.Core.Interfaces.Publishing;
using RealSync.Core.Models.Publishing;
using RealSync.Services.Publishing;

namespace RealSync.UnitTests.Publishing;

[TestFixture]
public class Phase8ConnectorTests
{
    private Mock<ICredentialVault> _vaultMock = null!;

    [SetUp]
    public void SetUp()
    {
        _vaultMock = new Mock<ICredentialVault>();
        _vaultMock.Setup(v => v.Decrypt("enc", "v1")).Returns("access_token");
    }

    [Test]
    public async Task YouTubeValidateAsync_WithoutVideoUrl_ShouldFail()
    {
        var connector = new YouTubeConnector(
            new HttpClient(),
            _vaultMock.Object,
            Mock.Of<ILogger<YouTubeConnector>>());

        var result = await connector.ValidateAsync(new PublicationContext
        {
            Account = ActiveAccount(PublishingChannelType.YouTube, "YouTube"),
            Job = new PublicationJob { PublishMode = PublishMode.Direct },
            Variant = new ContentVariant { Title = "Video title" }
        }, CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("videoUrl"));
    }

    [Test]
    public void YouTubeMapStatus_WhenProcessingSucceeded_ShouldReturnPublished()
    {
        var result = YouTubeConnector.MapStatus(
            """{"items":[{"status":{"uploadStatus":"processed"},"processingDetails":{"processingStatus":"succeeded"}}]}""",
            "yt_123");

        result.Status.Should().Be(PublicationJobStatus.Published);
        result.PublishedUrl.Should().Be("https://www.youtube.com/watch?v=yt_123");
    }

    [Test]
    public async Task MetaPagePublishAsync_TextPost_ShouldReturnExternalPostId()
    {
        var handler = Handler(HttpStatusCode.OK, """{"id":"page_1_post_1","permalink_url":"https://facebook.com/page_1_post_1"}""");
        var connector = new MetaPageConnector(
            new HttpClient(handler.Object) { BaseAddress = new Uri("https://graph.facebook.com/v25.0/") },
            _vaultMock.Object,
            Mock.Of<ILogger<MetaPageConnector>>());

        var result = await connector.PublishAsync(new PublicationContext
        {
            Account = ActiveAccount(PublishingChannelType.FacebookPage, "Facebook"),
            Job = new PublicationJob { PublishMode = PublishMode.Direct },
            Variant = new ContentVariant { Caption = "Caption", LinkUrl = "https://realsync.vn/post" }
        }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.ExternalPostId.Should().Be("page_1_post_1");
        result.PublishedUrl.Should().Be("https://facebook.com/page_1_post_1");
    }

    [Test]
    public async Task InstagramPublishAsync_VideoContainer_ShouldReturnRemoteProcessing()
    {
        var handler = Handler(HttpStatusCode.OK, """{"id":"container_123"}""");
        var connector = new InstagramProfessionalConnector(
            new HttpClient(handler.Object) { BaseAddress = new Uri("https://graph.facebook.com/v25.0/") },
            _vaultMock.Object,
            Mock.Of<ILogger<InstagramProfessionalConnector>>());

        var result = await connector.PublishAsync(new PublicationContext
        {
            Account = ActiveAccount(PublishingChannelType.InstagramProfessional, "Instagram"),
            Job = new PublicationJob
            {
                PublishMode = PublishMode.Direct,
                MediaManifestJson = """{"videoUrl":"https://cdn.example.com/reel.mp4"}"""
            },
            Variant = new ContentVariant { Caption = "Reel caption" }
        }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.RequiresRemoteProcessing.Should().BeTrue();
        result.ExternalPublishId.Should().Be("container_123");
    }

    [Test]
    public async Task PortalPublishAsync_Assisted_ShouldNotReturnPublished()
    {
        var connector = new PortalConnector();
        var job = new PublicationJob { Id = Guid.NewGuid(), PublishMode = PublishMode.Assisted };

        var publishResult = await connector.PublishAsync(new PublicationContext
        {
            Job = job,
            Variant = new ContentVariant { Title = "Title", Caption = "Caption" }
        }, CancellationToken.None);
        var status = await connector.GetStatusAsync(new PublicationTrackingContext { Job = job }, CancellationToken.None);

        publishResult.RequiresRemoteProcessing.Should().BeTrue();
        status.Status.Should().Be(PublicationJobStatus.NeedsReview);
    }

    [Test]
    public async Task FacebookGroupValidateAsync_DirectMode_ShouldFail()
    {
        var connector = new FacebookGroupConnector();

        var result = await connector.ValidateAsync(new PublicationContext
        {
            Job = new PublicationJob { PublishMode = PublishMode.Direct },
            Variant = new ContentVariant { Caption = "Caption" }
        }, CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Assisted"));
    }

    private static ConnectedAccount ActiveAccount(PublishingChannelType channelType, string provider)
        => new()
        {
            Status = CredentialStatus.Active,
            Provider = provider,
            ChannelType = channelType,
            ExternalAccountId = "external_1",
            OAuthCredential = new OAuthCredential
            {
                AccessTokenEncrypted = "enc",
                EncryptionKeyVersion = "v1"
            }
        };

    private static Mock<HttpMessageHandler> Handler(HttpStatusCode statusCode, string body)
    {
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(body)
            });
        return handler;
    }
}
