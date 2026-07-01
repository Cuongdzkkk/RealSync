using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RealSync.Core.Interfaces;
using RealSync.Data.Context;
using RealSync.Services.Implementations;
using RealSync.Services.Options;
using RealSync.Shared.Exceptions;
using System.Text;

namespace RealSync.UnitTests.Storage;

[TestFixture]
public class LocalFileStorageServiceTests
{
    private RealSyncDbContext _dbContext = null!;
    private Mock<ICurrentUserService> _currentUserMock = null!;
    private Mock<ILogger<LocalFileStorageService>> _loggerMock = null!;
    private IOptions<StorageOptions> _options = null!;
    private string _tempTestDir = null!;

    [SetUp]
    public void SetUp()
    {
        // Setup in-memory db
        var dbOptions = new DbContextOptionsBuilder<RealSyncDbContext>()
            .UseInMemoryDatabase($"realsync-storage-tests-{Guid.NewGuid():N}")
            .Options;
        _dbContext = new RealSyncDbContext(dbOptions);

        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        _currentUserMock.SetupGet(x => x.IsAuthenticated).Returns(true);

        _loggerMock = new Mock<ILogger<LocalFileStorageService>>();

        // Setup local temp path for test execution
        _tempTestDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"TestStorage_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempTestDir);

        var storageOptions = new StorageOptions
        {
            RootPath = _tempTestDir,
            PublicPath = Path.Combine(_tempTestDir, "Uploads/Public"),
            PrivatePath = Path.Combine(_tempTestDir, "Uploads/Private"),
            TempPath = Path.Combine(_tempTestDir, "Temp"),
            MinimumFreeSpaceBytes = 1024 * 1024, // 1MB for testing
            MaxPublicImageSizeMb = 1,
            MaxPrivateDocumentSizeMb = 2
        };
        _options = Microsoft.Extensions.Options.Options.Create(storageOptions);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
        if (Directory.Exists(_tempTestDir))
        {
            try
            {
                Directory.Delete(_tempTestDir, true);
            }
            catch
            {
                // Ignore cleanup lock issues in tests
            }
        }
    }

    private LocalFileStorageService CreateService(IOptions<StorageOptions>? customOptions = null)
    {
        return new LocalFileStorageService(
            customOptions ?? _options,
            _dbContext,
            _currentUserMock.Object,
            _loggerMock.Object
        );
    }

    [Test]
    public async Task SavePublicImageAsync_ShouldAcceptValidImage()
    {
        // JPEG magic bytes: FF D8 FF
        byte[] validJpegData = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46 };
        using var stream = new MemoryStream(validJpegData);
        var service = CreateService();

        var result = await service.SavePublicImageAsync(
            stream,
            "test_image.jpg",
            "image/jpeg",
            "properties"
        );

        result.Should().NotBeNull();
        result.OriginalFileName.Should().Be("test_image.jpg");
        result.ContentType.Should().Be("image/jpeg");
        result.Url.Should().StartWith("/uploads/properties/");

        // Check if metadata exists in db
        var meta = await _dbContext.StoredFiles.FindAsync(result.Id);
        meta.Should().NotBeNull();
        meta!.OriginalFileName.Should().Be("test_image.jpg");
        meta.IsPublic.Should().BeTrue();
    }

    [Test]
    public async Task SavePublicImageAsync_ShouldRejectExeFile()
    {
        byte[] someData = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 };
        using var stream = new MemoryStream(someData);
        var service = CreateService();

        var act = () => service.SavePublicImageAsync(
            stream,
            "virus.exe",
            "application/x-msdownload",
            "properties"
        );

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*không an toàn*");
    }

    [Test]
    public async Task SavePublicImageAsync_ShouldRejectFileExceedingLimit()
    {
        // 1.5MB exceeds our 1MB limit for testing
        byte[] largeData = new byte[1600 * 1024]; 
        using var stream = new MemoryStream(largeData);
        var service = CreateService();

        var act = () => service.SavePublicImageAsync(
            stream,
            "large_image.jpg",
            "image/jpeg",
            "properties"
        );

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*vượt quá giới hạn*");
    }

    [Test]
    public async Task SavePublicImageAsync_ShouldRejectPathTraversalName()
    {
        byte[] someData = new byte[] { 0x00, 0x01, 0x02, 0x03 };
        using var stream = new MemoryStream(someData);
        var service = CreateService();

        var act = () => service.SavePublicImageAsync(
            stream,
            "../../test.jpg",
            "image/jpeg",
            "properties"
        );

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*path traversal*");
    }

    [Test]
    public async Task SavePublicImageAsync_ShouldValidateMagicBytes()
    {
        // Declared as PNG but contains random bytes
        byte[] invalidData = new byte[] { 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88 };
        using var stream = new MemoryStream(invalidData);
        var service = CreateService();

        var act = () => service.SavePublicImageAsync(
            stream,
            "photo.png",
            "image/png",
            "properties"
        );

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*magic bytes*");
    }

    [Test]
    public async Task CheckHealthAsync_ShouldReturnUnhealthyWhenDriveDoesNotExist()
    {
        var invalidOptions = new StorageOptions
        {
            RootPath = @"Z:\NonExistentRealSyncDataPath",
            PublicPath = @"Z:\NonExistentRealSyncDataPath\Public",
            PrivatePath = @"Z:\NonExistentRealSyncDataPath\Private",
            TempPath = @"Z:\NonExistentRealSyncDataPath\Temp"
        };
        var service = CreateService(Options.Create(invalidOptions));

        var health = await service.CheckHealthAsync();

        health.StorageAvailable.Should().BeFalse();
        health.Status.Should().Be("Unhealthy");
    }
}
