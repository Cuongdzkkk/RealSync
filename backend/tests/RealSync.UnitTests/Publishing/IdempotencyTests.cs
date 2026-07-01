using System;
using FluentAssertions;
using NUnit.Framework;
using RealSync.Core.Helpers;

namespace RealSync.UnitTests.Publishing;

[TestFixture]
public class IdempotencyTests
{
    [Test]
    public void GenerateKey_IdenticalInputs_ShouldProduceIdenticalHash()
    {
        var workspaceId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var contentVariantId = Guid.NewGuid();
        var connectedAccountId = Guid.NewGuid();
        var scheduledAt = DateTime.UtcNow;
        var mediaManifest = "[\"image1.png\"]";

        var key1 = IdempotencyHelper.GenerateKey(workspaceId, postId, contentVariantId, connectedAccountId, scheduledAt, mediaManifest);
        var key2 = IdempotencyHelper.GenerateKey(workspaceId, postId, contentVariantId, connectedAccountId, scheduledAt, mediaManifest);

        key1.Should().NotBeNullOrEmpty();
        key1.Should().Be(key2);
    }

    [Test]
    public void GenerateKey_DifferentInputs_ShouldProduceDifferentHash()
    {
        var workspaceId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var contentVariantId = Guid.NewGuid();
        var connectedAccountId = Guid.NewGuid();
        var scheduledAt = DateTime.UtcNow;
        var mediaManifest = "[\"image1.png\"]";

        var key1 = IdempotencyHelper.GenerateKey(workspaceId, postId, contentVariantId, connectedAccountId, scheduledAt, mediaManifest);
        // Slightly change scheduled time
        var key2 = IdempotencyHelper.GenerateKey(workspaceId, postId, contentVariantId, connectedAccountId, scheduledAt.AddMinutes(5), mediaManifest);
        // Slightly change media manifest
        var key3 = IdempotencyHelper.GenerateKey(workspaceId, postId, contentVariantId, connectedAccountId, scheduledAt, "[\"image2.png\"]");

        key1.Should().NotBe(key2);
        key1.Should().NotBe(key3);
        key2.Should().NotBe(key3);
    }
}
