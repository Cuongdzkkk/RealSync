using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using RealSync.Core.Entities;
using RealSync.Core.Enums;
using RealSync.Core.Interfaces.Publishing;
using RealSync.Core.Models.Publishing;
using RealSync.Services.Options;
using RealSync.Services.Publishing;

namespace RealSync.UnitTests.Publishing;

[TestFixture]
public class TikTokConnectorTests
{
    private Mock<HttpMessageHandler> _httpMessageHandlerMock = null!;
    private Mock<ICredentialVault> _vaultMock = null!;
    private Mock<ILogger<TikTokConnector>> _loggerMock = null!;
    private TikTokConnector _connector = null!;

    [SetUp]
    public void SetUp()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://open.tiktokapis.com/")
        };
        _vaultMock = new Mock<ICredentialVault>();
        _loggerMock = new Mock<ILogger<TikTokConnector>>();
        var options = Options.Create(new TikTokOptions { IsAppAudited = false });
        _connector = new TikTokConnector(httpClient, _vaultMock.Object, options, _loggerMock.Object);
    }

    [Test]
    public async Task GetCapabilitiesAsync_UnauditedApp_ShouldDisableDirectPublish()
    {
        var account = new ConnectedAccount
        {
            Status = CredentialStatus.Active,
            GrantedScopesJson = "[\"video.upload\",\"video.publish\"]"
        };

        var caps = await _connector.GetCapabilitiesAsync(account, CancellationToken.None);

        caps.SupportsDraftUpload.Should().BeTrue();
        caps.SupportsDirectPublish.Should().BeFalse();
        caps.IsAppAudited.Should().BeFalse();
        caps.RestrictionReason.Should().Contain("audit");
    }

    [Test]
    public async Task GetCapabilitiesAsync_AuditedAppWithPublishScope_ShouldEnableDirectPublish()
    {
        var options = Options.Create(new TikTokOptions { IsAppAudited = true });
        var connector = new TikTokConnector(
            new HttpClient(_httpMessageHandlerMock.Object) { BaseAddress = new Uri("https://open.tiktokapis.com/") },
            _vaultMock.Object,
            options,
            _loggerMock.Object);

        var account = new ConnectedAccount
        {
            Status = CredentialStatus.Active,
            GrantedScopesJson = "[\"video.upload\",\"video.publish\"]"
        };

        var caps = await connector.GetCapabilitiesAsync(account, CancellationToken.None);

        caps.SupportsDirectPublish.Should().BeTrue();
        caps.SupportsDraftUpload.Should().BeTrue();
    }

    [Test]
    public async Task ValidateAsync_WithoutUserConsent_ShouldFail()
    {
        var account = new ConnectedAccount { Status = CredentialStatus.Active };
        var context = new PublicationContext
        {
            Account = account,
            Job = new PublicationJob
            {
                PublishMode = PublishMode.DraftUpload,
                MediaManifestJson = """{"videoUrl":"https://cdn.example.com/v.mp4","userConsentConfirmed":false,"privacyLevel":"SELF_ONLY"}"""
            },
            Variant = new ContentVariant { Caption = "Test" }
        };

        var result = await _connector.ValidateAsync(context, CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("đồng ý"));
    }

    [Test]
    public async Task ValidateAsync_DirectPostWhenUnaudited_ShouldFail()
    {
        var account = new ConnectedAccount
        {
            Status = CredentialStatus.Active,
            GrantedScopesJson = "[\"video.upload\",\"video.publish\"]"
        };
        var context = new PublicationContext
        {
            Account = account,
            Job = new PublicationJob
            {
                PublishMode = PublishMode.Direct,
                MediaManifestJson = """{"videoUrl":"https://cdn.example.com/v.mp4","userConsentConfirmed":true,"privacyLevel":"SELF_ONLY"}"""
            },
            Variant = new ContentVariant { Caption = "Test" }
        };

        var result = await _connector.ValidateAsync(context, CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("audit"));
    }

    [Test]
    public async Task PublishAsync_DraftUpload_ShouldReturnRemoteProcessing()
    {
        var account = new ConnectedAccount
        {
            Status = CredentialStatus.Active,
            OAuthCredential = new OAuthCredential
            {
                AccessTokenEncrypted = "enc",
                EncryptionKeyVersion = "v1"
            }
        };
        var context = new PublicationContext
        {
            Account = account,
            Job = new PublicationJob
            {
                PublishMode = PublishMode.DraftUpload,
                MediaManifestJson = """{"videoUrl":"https://cdn.example.com/v.mp4","userConsentConfirmed":true,"privacyLevel":"SELF_ONLY","isAigc":true}"""
            },
            Variant = new ContentVariant { Caption = "Caption" }
        };

        _vaultMock.Setup(v => v.Decrypt("enc", "v1")).Returns("access_token");

        var initResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(
                """{"data":{"publish_id":"pub_123"},"error":{"code":"ok","message":"","log_id":"log1"}}""")
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("inbox/video/init")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(initResponse);

        var result = await _connector.PublishAsync(context, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.RequiresRemoteProcessing.Should().BeTrue();
        result.ExternalPublishId.Should().Be("pub_123");
    }

    [Test]
    public void MapApiFailure_SpamRisk_ShouldNotBeRetryable()
    {
        var apiResult = new TikTokApiResult<object>
        {
            IsSuccess = false,
            ErrorCode = "spam_risk",
            ErrorMessage = "Policy violation"
        };

        var result = TikTokConnector.MapApiFailure(apiResult);

        result.IsSuccess.Should().BeFalse();
        result.IsRetryable.Should().BeFalse();
        result.ErrorCategory.Should().Be("PolicyError");
    }

    [Test]
    public async Task GetStatusAsync_PublishComplete_ShouldReturnPublished()
    {
        var account = new ConnectedAccount
        {
            OAuthCredential = new OAuthCredential
            {
                AccessTokenEncrypted = "enc",
                EncryptionKeyVersion = "v1"
            }
        };
        var context = new PublicationTrackingContext
        {
            Account = account,
            Job = new PublicationJob
            {
                ExternalPublishId = "pub_123",
                PublishMode = PublishMode.DraftUpload
            }
        };

        _vaultMock.Setup(v => v.Decrypt("enc", "v1")).Returns("access_token");

        var statusResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(
                """{"data":{"status":"PUBLISH_COMPLETE","publicaly_available_post_id":["7123456789"]},"error":{"code":"ok"}}""")
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("status/fetch")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(statusResponse);

        var result = await _connector.GetStatusAsync(context, CancellationToken.None);

        result.Status.Should().Be(PublicationJobStatus.Published);
        result.PublishedUrl.Should().Contain("7123456789");
    }
}
