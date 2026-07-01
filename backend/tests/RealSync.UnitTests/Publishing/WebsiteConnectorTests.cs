using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using RealSync.Core.Entities;
using RealSync.Core.Enums;
using RealSync.Core.Models.Publishing;
using RealSync.Services.Publishing;

namespace RealSync.UnitTests.Publishing;

[TestFixture]
public class WebsiteConnectorTests
{
    private WebsiteConnector _connector = null!;

    [SetUp]
    public void SetUp()
    {
        _connector = new WebsiteConnector();
    }

    [Test]
    public async Task GetCapabilitiesAsync_ShouldReturnDefaultCapabilities()
    {
        var capabilities = await _connector.GetCapabilitiesAsync(null, CancellationToken.None);
        capabilities.Should().NotBeNull();
        capabilities.SupportsDirectPublish.Should().BeTrue();
        capabilities.SupportsScheduling.Should().BeTrue();
        capabilities.SupportsImages.Should().BeTrue();
        capabilities.SupportsVideo.Should().BeFalse();
    }

    [Test]
    public async Task ValidateAsync_WithValidVariant_ShouldReturnSuccess()
    {
        var variant = new ContentVariant
        {
            Title = "Nhà mặt tiền Quận 1",
            Caption = "Cơ hội sở hữu nhà đẹp Quận 1, pháp lý đầy đủ..."
        };
        var context = new PublicationContext { Variant = variant };

        var result = await _connector.ValidateAsync(context, CancellationToken.None);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Test]
    [TestCase("", "Nội dung", "Tiêu đề nội dung trống.")]
    [TestCase("Tiêu đề", "", "Nội dung bài viết trống.")]
    [TestCase(null, "Nội dung", "Tiêu đề nội dung trống.")]
    [TestCase("Tiêu đề", null, "Nội dung bài viết trống.")]
    public async Task ValidateAsync_WithInvalidVariant_ShouldReturnFailure(string? title, string? caption, string expectedError)
    {
        var variant = new ContentVariant
        {
            Title = title,
            Caption = caption
        };
        var context = new PublicationContext { Variant = variant };

        var result = await _connector.ValidateAsync(context, CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(expectedError);
    }

    [Test]
    public async Task PublishAsync_ShouldReturnLocalShowcaseUrl()
    {
        var postId = Guid.NewGuid();
        var job = new PublicationJob { PostId = postId };
        var context = new PublicationContext { Job = job };

        var result = await _connector.PublishAsync(context, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.ExternalPostId.Should().Be(postId.ToString());
        result.PublishedUrl.Should().Be($"/api/v1/posts/{postId}/public");
    }

    [Test]
    public async Task GetStatusAsync_ShouldReturnPublishedWithLocalUrl()
    {
        var postId = Guid.NewGuid();
        var job = new PublicationJob { PostId = postId };
        var context = new PublicationTrackingContext { Job = job };

        var result = await _connector.GetStatusAsync(context, CancellationToken.None);

        result.Status.Should().Be(PublicationJobStatus.Published);
        result.PublishedUrl.Should().Be($"/api/v1/posts/{postId}/public");
    }
}
