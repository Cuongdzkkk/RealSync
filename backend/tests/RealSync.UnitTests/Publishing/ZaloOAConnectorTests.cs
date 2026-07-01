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
public class ZaloOAConnectorTests
{
    private Mock<HttpMessageHandler> _httpMessageHandlerMock = null!;
    private Mock<ICredentialVault> _vaultMock = null!;
    private Mock<ILogger<ZaloOAConnector>> _loggerMock = null!;
    private ZaloOAConnector _connector = null!;

    [SetUp]
    public void SetUp()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://openapi.zalo.me/")
        };
        _vaultMock = new Mock<ICredentialVault>();
        _loggerMock = new Mock<ILogger<ZaloOAConnector>>();
        _connector = new ZaloOAConnector(httpClient, _vaultMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task GetCapabilitiesAsync_ShouldReturnZaloOACapabilities()
    {
        var capabilities = await _connector.GetCapabilitiesAsync(null, CancellationToken.None);
        capabilities.Should().NotBeNull();
        capabilities.SupportsDirectPublish.Should().BeTrue();
        capabilities.SupportsImages.Should().BeTrue();
        capabilities.SupportsVideo.Should().BeFalse();
        capabilities.SupportsScheduling.Should().BeFalse();
        capabilities.SupportsDelete.Should().BeFalse();
    }

    [Test]
    public async Task ValidateAsync_WithNullAccount_ShouldReturnFailure()
    {
        var context = new PublicationContext
        {
            Account = null,
            Variant = new ContentVariant { Title = "Title", Caption = "Caption" }
        };

        var result = await _connector.ValidateAsync(context, CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Chưa liên kết tài khoản"));
    }

    [Test]
    [TestCase(CredentialStatus.PendingSetup, "chưa được thiết lập")]
    [TestCase(CredentialStatus.Expired, "đã hết hạn")]
    [TestCase(CredentialStatus.Invalid, "không hợp lệ")]
    [TestCase(CredentialStatus.Revoked, "không hợp lệ")]
    public async Task ValidateAsync_WithNonActiveAccountStatus_ShouldReturnFailure(CredentialStatus status, string expectedErrorSub)
    {
        var account = new ConnectedAccount { Status = status };
        var context = new PublicationContext
        {
            Account = account,
            Variant = new ContentVariant { Title = "Title", Caption = "Caption" }
        };

        var result = await _connector.ValidateAsync(context, CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains(expectedErrorSub));
    }

    [Test]
    [TestCase("", "Caption", "Tiêu đề")]
    [TestCase("Title", "", "Nội dung")]
    [TestCase(null, "Caption", "Tiêu đề")]
    [TestCase("Title", null, "Nội dung")]
    public async Task ValidateAsync_WithEmptyContent_ShouldReturnFailure(string? title, string? caption, string expectedErrorSub)
    {
        var account = new ConnectedAccount { Status = CredentialStatus.Active };
        var context = new PublicationContext
        {
            Account = account,
            Variant = new ContentVariant { Title = title, Caption = caption }
        };

        var result = await _connector.ValidateAsync(context, CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains(expectedErrorSub));
    }

    [Test]
    public async Task ValidateAsync_WithLongTitle_ShouldReturnFailure()
    {
        var account = new ConnectedAccount { Status = CredentialStatus.Active };
        var context = new PublicationContext
        {
            Account = account,
            Variant = new ContentVariant { Title = new string('A', 201), Caption = "Caption" }
        };

        var result = await _connector.ValidateAsync(context, CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("vượt quá 200 ký tự"));
    }

    [Test]
    public async Task ValidateAsync_WithValidData_ShouldReturnSuccess()
    {
        var account = new ConnectedAccount { Status = CredentialStatus.Active };
        var context = new PublicationContext
        {
            Account = account,
            Variant = new ContentVariant { Title = "Valid Title", Caption = "Valid Caption" }
        };

        var result = await _connector.ValidateAsync(context, CancellationToken.None);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Test]
    public async Task PublishAsync_WithDecryptionFailure_ShouldReturnAuthenticationError()
    {
        var account = new ConnectedAccount
        {
            Status = CredentialStatus.Active,
            OAuthCredential = new OAuthCredential
            {
                AccessTokenEncrypted = "token_encrypted",
                EncryptionKeyVersion = "v1"
            }
        };
        var context = new PublicationContext
        {
            Account = account,
            Variant = new ContentVariant { Title = "Title", Caption = "Caption" }
        };

        _vaultMock.Setup(v => v.Decrypt("token_encrypted", "v1"))
            .Throws(new Exception("Decryption failed"));

        var result = await _connector.PublishAsync(context, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("DECRYPTION_FAILED");
        result.ErrorCategory.Should().Be("AuthenticationError");
    }

    [Test]
    public async Task PublishAsync_WithSuccessFlow_ShouldReturnArticleToken()
    {
        var account = new ConnectedAccount
        {
            Status = CredentialStatus.Active,
            OAuthCredential = new OAuthCredential
            {
                AccessTokenEncrypted = "token_encrypted",
                EncryptionKeyVersion = "v1"
            }
        };
        var context = new PublicationContext
        {
            Account = account,
            Variant = new ContentVariant { Title = "Title", Caption = "Caption" }
        };

        _vaultMock.Setup(v => v.Decrypt("token_encrypted", "v1"))
            .Returns("decrypted_token");

        // Mock 1st call: Create Article
        var createResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"error\": 0, \"message\": \"Success\", \"data\": {\"token\": \"zalo_article_token_123\"}}")
        };

        // Mock 2nd call: Verify Article
        var verifyResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"error\": 0, \"message\": \"Success\"}")
        };

        var handlerSequence = _httpMessageHandlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());

        handlerSequence.ReturnsAsync(createResponse);
        handlerSequence.ReturnsAsync(verifyResponse);

        var result = await _connector.PublishAsync(context, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.ExternalPostId.Should().Be("zalo_article_token_123");
    }

    [Test]
    public async Task PublishAsync_WhenCreateArticleFails_ShouldReturnFailure()
    {
        var account = new ConnectedAccount
        {
            Status = CredentialStatus.Active,
            OAuthCredential = new OAuthCredential
            {
                AccessTokenEncrypted = "token_encrypted",
                EncryptionKeyVersion = "v1"
            }
        };
        var context = new PublicationContext
        {
            Account = account,
            Variant = new ContentVariant { Title = "Title", Caption = "Caption" }
        };

        _vaultMock.Setup(v => v.Decrypt("token_encrypted", "v1"))
            .Returns("decrypted_token");

        var createFailureResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"error\": -201, \"message\": \"Invalid token\"}")
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(createFailureResponse);

        var result = await _connector.PublishAsync(context, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_TOKEN");
        result.ErrorCategory.Should().Be("AuthenticationError");
    }

    [Test]
    public async Task DeleteAsync_ShouldAlwaysReturnFailure()
    {
        var context = new PublicationTrackingContext();
        var result = await _connector.DeleteAsync(context, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("không hỗ trợ xóa bài viết");
    }

    [Test]
    [TestCase(-201, "INVALID_TOKEN", "AuthenticationError")]
    [TestCase(-202, "TOKEN_EXPIRED", "AuthenticationError")]
    [TestCase(-210, "RATE_LIMIT_EXCEEDED", "RateLimitError")]
    [TestCase(-213, "PERMISSION_DENIED", "AuthorizationError")]
    [TestCase(-216, "OA_NOT_FOUND", "ConfigurationError")]
    [TestCase(-217, "OA_NOT_ACTIVE", "ConfigurationError")]
    [TestCase(-231, "INVALID_MEDIA", "ValidationError")]
    [TestCase(-232, "MEDIA_TOO_LARGE", "ValidationError")]
    [TestCase(-240, "INVALID_ARTICLE", "ValidationError")]
    [TestCase(-241, "ARTICLE_NOT_FOUND", "ValidationError")]
    [TestCase(-999, "ZALO_999", "UnknownError")]
    public void MapZaloError_ShouldMapCorrectly(int errorCode, string expectedCode, string expectedCategory)
    {
        var result = ZaloOAConnector.MapZaloError(errorCode, "Test error message");
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(expectedCode);
        result.ErrorCategory.Should().Be(expectedCategory);
    }
}
