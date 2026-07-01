using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RealSync.Core.Entities;
using RealSync.Core.Interfaces;
using RealSync.Core.Interfaces.Publishing;
using RealSync.Data.Context;
using RealSync.Services.Implementations;
using RealSync.Services.Options;
using RealSync.Shared.DTOs.Requests.Posts;
using RealSync.Shared.Exceptions;

namespace RealSync.UnitTests.Publishing;

[TestFixture]
public class AIContentServiceTests
{
    private RealSyncDbContext _context = null!;
    private Mock<ICurrentUserService> _currentUserMock = null!;
    private Mock<IAITextProvider> _aiProviderMock = null!;
    private Mock<ILogger<AIContentService>> _loggerMock = null!;
    private AIOptions _aiOptions = null!;
    private AIContentService _service = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<RealSyncDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new RealSyncDbContext(options);
        _currentUserMock = new Mock<ICurrentUserService>();
        _aiProviderMock = new Mock<IAITextProvider>();
        _loggerMock = new Mock<ILogger<AIContentService>>();
        
        _aiOptions = new AIOptions
        {
            Provider = "Gemini",
            ApiKey = "test-api-key",
            Model = "gemini-2.0-flash"
        };

        _currentUserMock.Setup(u => u.Email).Returns("agent@realsync.vn");

        _service = new AIContentService(
            _context,
            _currentUserMock.Object,
            _aiProviderMock.Object,
            Options.Create(_aiOptions),
            _loggerMock.Object
        );
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }

    [Test]
    public async Task ApplyAsync_ShouldUpdatePostContentAndSummary()
    {
        // Arrange
        var post = new Post
        {
            Id = Guid.NewGuid(),
            Title = "BĐS Quận 2",
            AuthorId = Guid.NewGuid()
        };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        var request = new ApplyAIContentRequest("Nội dung bài viết mới đã được chỉnh sửa", "Tóm tắt ngắn gọn");

        // Act
        await _service.ApplyAsync(post.Id, request);

        // Assert
        var updatedPost = await _context.Posts.FindAsync(post.Id);
        updatedPost.Should().NotBeNull();
        updatedPost!.Content.Should().Be("Nội dung bài viết mới đã được chỉnh sửa");
        updatedPost.Summary.Should().Be("Tóm tắt ngắn gọn");
    }

    [Test]
    public async Task GenerateAsync_ShouldCallAITextProvider_WhenConfigured()
    {
        // Arrange
        var post = new Post
        {
            Id = Guid.NewGuid(),
            Title = "Căn hộ Vinhomes",
            AuthorId = Guid.NewGuid()
        };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        var expectedOutput = new StructuredAIOutput
        {
            Title = "Căn hộ Vinhomes",
            Summary = "Căn hộ Vinhomes giá tốt",
            Caption = "Cơ hội sở hữu căn hộ đẹp",
            Hashtags = new List<string> { "Vinhomes", "CanHo" },
            CallToAction = "Liên hệ 0909",
            FactsUsed = new List<FactItem> { new FactItem { Field = "price", Value = "Thỏa thuận" } },
            Warnings = new List<string> { "Pháp lý đang cập nhật" }
        };

        _aiProviderMock.Setup(p => p.GenerateStructuredAsync(It.IsAny<AITextRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StructuredAIOutputResult
            {
                Output = expectedOutput,
                PromptTokens = 100,
                CompletionTokens = 200
            });

        var request = new AIContentGenerateRequest { Prompt = "Viết tin đăng Vinhomes" };

        // Act
        var response = await _service.GenerateAsync(post.Id, request);

        // Assert
        response.Should().NotBeNull();
        response.GeneratedContent.Should().Contain("Cơ hội sở hữu căn hộ đẹp");
        response.PromptTokens.Should().Be(100);
        response.CompletionTokens.Should().Be(200);
        response.EstimatedCost.Should().Be((100 * 0.000000075m) + (200 * 0.0000003m));
        response.FactsUsedJson.Should().Contain("price");
    }

    [Test]
    public void GenerateAsync_ShouldThrowException_WhenAIFailsAndNotInDevelopment()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        
        var post = new Post
        {
            Id = Guid.NewGuid(),
            Title = "Căn hộ Vinhomes",
            AuthorId = Guid.NewGuid()
        };
        _context.Posts.Add(post);
        _context.SaveChanges();

        _aiProviderMock.Setup(p => p.GenerateStructuredAsync(It.IsAny<AITextRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("API Key expired"));

        var request = new AIContentGenerateRequest { Prompt = "Viết tin đăng Vinhomes" };

        // Act & Assert
        Assert.ThrowsAsync<BusinessException>(async () =>
        {
            await _service.GenerateAsync(post.Id, request);
        });
    }
}
