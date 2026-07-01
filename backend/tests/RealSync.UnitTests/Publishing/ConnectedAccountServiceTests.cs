using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RealSync.Core.Entities;
using RealSync.Core.Enums;
using RealSync.Core.Interfaces;
using RealSync.Core.Interfaces.Publishing;
using RealSync.Data.Context;
using RealSync.Services.Publishing;
using RealSync.Shared.DTOs.Requests.Publishing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RealSync.Services.Options;

namespace RealSync.UnitTests.Publishing;

[TestFixture]
public class ConnectedAccountServiceTests
{
    private RealSyncDbContext _context = null!;
    private Mock<ICredentialVault> _vaultMock = null!;
    private Mock<IActivityLogService> _activityLogMock = null!;
    private Mock<ICurrentUserService> _currentUserMock = null!;
    private ConnectedAccountService _service = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<RealSyncDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new RealSyncDbContext(options);
        _vaultMock = new Mock<ICredentialVault>();
        _activityLogMock = new Mock<IActivityLogService>();
        _currentUserMock = new Mock<ICurrentUserService>();

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        var configurationMock = new Mock<IConfiguration>();
        var loggerMock = new Mock<ILogger<ZaloTokenRefreshService>>();
        var refreshServiceMock = new Mock<ZaloTokenRefreshService>(scopeFactoryMock.Object, configurationMock.Object, loggerMock.Object);

        var tikTokOptions = Options.Create(new TikTokOptions { IsAppAudited = false });
        var tikTokOAuthMock = new Mock<TikTokOAuthService>(
            Mock.Of<HttpClient>(),
            tikTokOptions,
            Mock.Of<Microsoft.Extensions.Caching.Memory.IMemoryCache>(),
            Mock.Of<ILogger<TikTokOAuthService>>());
        var tikTokRefreshMock = new Mock<TikTokTokenRefreshService>(
            scopeFactoryMock.Object,
            Mock.Of<ILogger<TikTokTokenRefreshService>>());
        var tikTokConnectorMock = new Mock<TikTokConnector>(
            Mock.Of<HttpClient>(),
            _vaultMock.Object,
            tikTokOptions,
            Mock.Of<ILogger<TikTokConnector>>());
        var connectorResolverMock = new Mock<IConnectorResolver>();
        var serviceLogger = Mock.Of<ILogger<ConnectedAccountService>>();

        _service = new ConnectedAccountService(
            _context,
            _vaultMock.Object,
            _activityLogMock.Object,
            _currentUserMock.Object,
            connectorResolverMock.Object,
            refreshServiceMock.Object,
            tikTokOAuthMock.Object,
            tikTokRefreshMock.Object,
            tikTokConnectorMock.Object,
            tikTokOptions,
            serviceLogger
        );
        
        _vaultMock.Setup(v => v.Encrypt(It.IsAny<string>()))
            .Returns((string text) => (text + "_encrypted", "v1"));
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task CreateAsync_ShouldEncryptTokensAndSaveToDb()
    {
        // Arrange
        var request = new ConnectedAccountCreateRequest(
            Provider: "Facebook",
            ChannelType: PublishingChannelType.FacebookPage,
            DisplayName: "My Page",
            ExternalAccountId: "fb-12345",
            ExternalParentAccountId: null,
            ProfileUrl: "https://facebook.com/mypage",
            AvatarUrl: "https://avatar.com/1",
            AccessToken: "my_raw_access_token",
            RefreshToken: "my_raw_refresh_token",
            ExpiresInSeconds: 3600,
            GrantedScopesJson: "[\"pages_show_list\"]"
        );

        // Act
        var result = await _service.CreateAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Provider.Should().Be("Facebook");
        result.DisplayName.Should().Be("My Page");
        result.Status.Should().Be(CredentialStatus.Active);

        // Verify DB ConnectedAccount entry
        var dbAccount = await _context.ConnectedAccounts.FirstOrDefaultAsync(a => a.Id == result.Id);
        dbAccount.Should().NotBeNull();
        dbAccount!.ExternalAccountId.Should().Be("fb-12345");
        dbAccount.Status.Should().Be(CredentialStatus.Active);

        // Verify DB OAuthCredential entry
        var dbCredential = await _context.OAuthCredentials.FirstOrDefaultAsync(c => c.ConnectedAccountId == result.Id);
        dbCredential.Should().NotBeNull();
        dbCredential!.AccessTokenEncrypted.Should().Be("my_raw_access_token_encrypted");
        dbCredential.RefreshTokenEncrypted.Should().Be("my_raw_refresh_token_encrypted");
        dbCredential.EncryptionKeyVersion.Should().Be("v1");

        // Verify activity log triggered
        _activityLogMock.Verify(l => l.LogAsync(
            "ConnectedAccount",
            result.Id,
            ActivityType.Create,
            It.Is<string>(d => d.Contains("Kết nối thành công tài khoản Facebook")),
            null,
            null
        ), Times.Once);
    }

    [Test]
    public async Task CreateAsync_WhenAccountAlreadyExists_ShouldThrowBusinessException()
    {
        // Arrange
        var request = new ConnectedAccountCreateRequest(
            Provider: "Zalo",
            ChannelType: PublishingChannelType.ZaloOA,
            DisplayName: "My OA",
            ExternalAccountId: "zalo-111",
            ExternalParentAccountId: null,
            ProfileUrl: null,
            AvatarUrl: null,
            AccessToken: "token",
            RefreshToken: null,
            ExpiresInSeconds: null,
            GrantedScopesJson: null
        );

        await _service.CreateAsync(request, CancellationToken.None);

        // Act & Assert
        Func<Task> act = async () => await _service.CreateAsync(request, CancellationToken.None);
        await act.Should().ThrowAsync<Exception>(); // BusinessException inherits from AppException/Exception
    }

    [Test]
    public async Task CheckHealthAsync_WhenTokenExpired_ShouldSetStatusToExpired()
    {
        // Arrange
        var request = new ConnectedAccountCreateRequest(
            Provider: "TikTok",
            ChannelType: PublishingChannelType.TikTok,
            DisplayName: "My TikTok",
            ExternalAccountId: "tt-222",
            ExternalParentAccountId: null,
            ProfileUrl: null,
            AvatarUrl: null,
            AccessToken: "token",
            RefreshToken: null,
            ExpiresInSeconds: -10, // already expired in past
            GrantedScopesJson: null
        );

        var acc = await _service.CreateAsync(request, CancellationToken.None);

        // Act
        var result = await _service.CheckHealthAsync(acc.Id, CancellationToken.None);

        // Assert
        result.Status.Should().Be(CredentialStatus.Expired);
        result.LastErrorCode.Should().Be("TOKEN_EXPIRED");

        var dbAcc = await _context.ConnectedAccounts.FindAsync(acc.Id);
        dbAcc!.Status.Should().Be(CredentialStatus.Expired);

        var dbCred = await _context.OAuthCredentials.FirstOrDefaultAsync(c => c.ConnectedAccountId == acc.Id);
        dbCred!.CredentialStatus.Should().Be(CredentialStatus.Expired);
    }

    [Test]
    public async Task ReconnectAsync_ShouldUpdateTokensAndResetStatusToActive()
    {
        // Arrange
        var request = new ConnectedAccountCreateRequest(
            Provider: "Facebook",
            ChannelType: PublishingChannelType.FacebookPage,
            DisplayName: "My FB",
            ExternalAccountId: "fb-777",
            ExternalParentAccountId: null,
            ProfileUrl: null,
            AvatarUrl: null,
            AccessToken: "old_token",
            RefreshToken: null,
            ExpiresInSeconds: -10, // expired
            GrantedScopesJson: null
        );

        var acc = await _service.CreateAsync(request, CancellationToken.None);
        await _service.CheckHealthAsync(acc.Id, CancellationToken.None); // transitions to Expired

        var reconnectReq = new ConnectedAccountReconnectRequest(
            AccessToken: "new_valid_token",
            RefreshToken: "new_refresh_token",
            ExpiresInSeconds: 3600
        );

        // Act
        var result = await _service.ReconnectAsync(acc.Id, reconnectReq, CancellationToken.None);

        // Assert
        result.Status.Should().Be(CredentialStatus.Active);
        result.LastErrorCode.Should().BeNull();
        result.TokenExpiresAt.Should().BeAfter(DateTime.UtcNow);

        var dbCred = await _context.OAuthCredentials.FirstOrDefaultAsync(c => c.ConnectedAccountId == acc.Id);
        dbCred!.AccessTokenEncrypted.Should().Be("new_valid_token_encrypted");
        dbCred.RefreshTokenEncrypted.Should().Be("new_refresh_token_encrypted");
        dbCred.CredentialStatus.Should().Be(CredentialStatus.Active);
    }

    [Test]
    public async Task DeleteAsync_ShouldRemoveAccountAndCredential()
    {
        // Arrange
        var request = new ConnectedAccountCreateRequest(
            Provider: "Zalo",
            ChannelType: PublishingChannelType.ZaloOA,
            DisplayName: "Delete Me",
            ExternalAccountId: "zalo-del",
            ExternalParentAccountId: null,
            ProfileUrl: null,
            AvatarUrl: null,
            AccessToken: "token",
            RefreshToken: null,
            ExpiresInSeconds: null,
            GrantedScopesJson: null
        );

        var acc = await _service.CreateAsync(request, CancellationToken.None);

        // Act
        await _service.DeleteAsync(acc.Id, CancellationToken.None);

        // Assert
        var dbAcc = await _context.ConnectedAccounts.FindAsync(acc.Id);
        dbAcc.Should().BeNull();

        var dbCred = await _context.OAuthCredentials.FirstOrDefaultAsync(c => c.ConnectedAccountId == acc.Id);
        dbCred.Should().BeNull();
    }
}
