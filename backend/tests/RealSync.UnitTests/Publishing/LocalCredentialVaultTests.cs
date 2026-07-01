using System;
using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RealSync.Core.Models.Publishing;
using RealSync.Services.Publishing;

namespace RealSync.UnitTests.Publishing;

[TestFixture]
public class LocalCredentialVaultTests
{
    private Mock<IConfiguration> _configMock = null!;

    [SetUp]
    public void SetUp()
    {
        _configMock = new Mock<IConfiguration>();
    }

    [Test]
    public void Encrypt_And_Decrypt_ShouldWorkCorrectly_WithConfiguredKey()
    {
        // Arrange
        var options = Options.Create(new VaultOptions
        {
            MasterKey = "MySuperSecretMasterKey123!",
            ActiveVersion = "v1"
        });
        var vault = new LocalCredentialVault(options, _configMock.Object);
        var plaintext = "Zalo-OAuth-Access-Token-Value-2026";

        // Act
        var (ciphertext, keyVersion) = vault.Encrypt(plaintext);
        var decrypted = vault.Decrypt(ciphertext, keyVersion);

        // Assert
        ciphertext.Should().NotBeNullOrEmpty();
        ciphertext.Should().NotBe(plaintext);
        keyVersion.Should().Be("v1");
        decrypted.Should().Be(plaintext);
    }

    [Test]
    public void Encrypt_And_Decrypt_ShouldWorkCorrectly_WithFallbackKey()
    {
        // Arrange
        var options = Options.Create(new VaultOptions
        {
            MasterKey = "", // empty to trigger fallback
            ActiveVersion = "v2"
        });
        _configMock.Setup(c => c["Jwt:Secret"]).Returns("JWT-Secret-Key-Used-For-Fallback-Encryption");
        var vault = new LocalCredentialVault(options, _configMock.Object);
        var plaintext = "Facebook-Page-Access-Token-456";

        // Act
        var (ciphertext, keyVersion) = vault.Encrypt(plaintext);
        var decrypted = vault.Decrypt(ciphertext, keyVersion);

        // Assert
        ciphertext.Should().NotBeNullOrEmpty();
        keyVersion.Should().Be("v2");
        decrypted.Should().Be(plaintext);
    }

    [Test]
    public void Encrypt_WithNullOrEmptyPlaintext_ShouldReturnEmptyCiphertext()
    {
        // Arrange
        var options = Options.Create(new VaultOptions
        {
            MasterKey = "SomeKey",
            ActiveVersion = "v1"
        });
        var vault = new LocalCredentialVault(options, _configMock.Object);

        // Act
        var (cipherNull, _) = vault.Encrypt(null!);
        var (cipherEmpty, _) = vault.Encrypt("");

        // Assert
        cipherNull.Should().BeEmpty();
        cipherEmpty.Should().BeEmpty();
    }

    [Test]
    public void Decrypt_WithNullOrEmptyCiphertext_ShouldReturnEmptyPlaintext()
    {
        // Arrange
        var options = Options.Create(new VaultOptions
        {
            MasterKey = "SomeKey",
            ActiveVersion = "v1"
        });
        var vault = new LocalCredentialVault(options, _configMock.Object);

        // Act
        var plainNull = vault.Decrypt(null!, "v1");
        var plainEmpty = vault.Decrypt("", "v1");

        // Assert
        plainNull.Should().BeEmpty();
        plainEmpty.Should().BeEmpty();
    }
}
