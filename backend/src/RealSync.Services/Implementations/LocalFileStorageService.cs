using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RealSync.Core.Entities;
using RealSync.Core.Interfaces;
using RealSync.Services.Options;
using RealSync.Data.Context;
using RealSync.Shared.Exceptions;

namespace RealSync.Services.Implementations;

public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly StorageOptions _options;
    private readonly RealSyncDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<LocalFileStorageService> _logger;

    private static readonly HashSet<string> ForbiddenExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".dll", ".cmd", ".bat", ".ps1", ".js", ".html", ".svg"
    };

    private static readonly Dictionary<string, string[]> AllowedPublicImages = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = new[] { "image/jpeg" },
        [".jpeg"] = new[] { "image/jpeg" },
        [".png"] = new[] { "image/png" },
        [".webp"] = new[] { "image/webp" },
        [".mp4"] = new[] { "video/mp4" }
    };

    private static readonly Dictionary<string, string[]> AllowedPrivateDocuments = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = new[] { "application/pdf" },
        [".docx"] = new[] { "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        [".xlsx"] = new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" }
    };

    public LocalFileStorageService(
        IOptions<StorageOptions> options,
        RealSyncDbContext dbContext,
        ICurrentUserService currentUserService,
        ILogger<LocalFileStorageService> logger)
    {
        _options = options.Value;
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<StoredFileResult> SavePublicImageAsync(
        Stream stream,
        string originalFileName,
        string contentType,
        string category,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting public image upload: {FileName}, Category: {Category}", originalFileName, category);
        
        var ext = Path.GetExtension(originalFileName).ToLowerInvariant();
        
        // 1. Validation
        ValidateFileSecurity(originalFileName, ext);
        if (!AllowedPublicImages.ContainsKey(ext))
        {
            throw new BusinessException($"Định dạng file '{ext}' không được hỗ trợ cho ảnh công khai.");
        }
        
        var allowedMimes = AllowedPublicImages[ext];
        if (!allowedMimes.Contains(contentType))
        {
            throw new BusinessException($"MIME type '{contentType}' không hợp lệ cho phần mở rộng '{ext}'.");
        }

        long maxSizeBytes = _options.MaxPublicImageSizeMb * 1024L * 1024L;

        // 2. Safe Write Flow
        return await SaveFileInternalAsync(stream, originalFileName, contentType, category, ext, maxSizeBytes, isPublic: true, cancellationToken);
    }

    public async Task<StoredFileResult> SavePrivateDocumentAsync(
        Stream stream,
        string originalFileName,
        string contentType,
        string category,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting private document upload: {FileName}, Category: {Category}", originalFileName, category);

        var ext = Path.GetExtension(originalFileName).ToLowerInvariant();

        // 1. Validation
        ValidateFileSecurity(originalFileName, ext);
        if (!AllowedPrivateDocuments.ContainsKey(ext))
        {
            throw new BusinessException($"Định dạng file '{ext}' không được hỗ trợ cho tài liệu riêng tư.");
        }

        var allowedMimes = AllowedPrivateDocuments[ext];
        if (!allowedMimes.Contains(contentType))
        {
            throw new BusinessException($"MIME type '{contentType}' không hợp lệ cho phần mở rộng '{ext}'.");
        }

        long maxSizeBytes = _options.MaxPrivateDocumentSizeMb * 1024L * 1024L;

        // 2. Safe Write Flow
        return await SaveFileInternalAsync(stream, originalFileName, contentType, category, ext, maxSizeBytes, isPublic: false, cancellationToken);
    }

    public async Task<Stream> OpenReadAsync(
        string relativePath,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Opening file for reading: {RelativePath}", relativePath);

        var isPublic = relativePath.StartsWith("Public/", StringComparison.OrdinalIgnoreCase);
        var baseDir = isPublic ? _options.PublicPath : _options.PrivatePath;
        
        // Remove Public/ or Private/ prefix to locate it under the baseDir
        var subPath = relativePath.Substring(isPublic ? "Public/".Length : "Private/".Length);
        var absolutePath = Path.Combine(baseDir, subPath);

        EnsureWithinStorageRoot(absolutePath, baseDir);

        if (!File.Exists(absolutePath))
        {
            throw new NotFoundException($"File tại đường dẫn '{relativePath}' không tồn tại.");
        }

        return await Task.FromResult(new FileStream(absolutePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true));
    }

    public async Task DeleteAsync(
        string relativePath,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting file: {RelativePath}", relativePath);

        // Soft delete metadata first
        var storedFile = _dbContext.StoredFiles.FirstOrDefault(f => f.RelativePath == relativePath && !f.IsDeleted);
        if (storedFile != null)
        {
            storedFile.IsDeleted = true;
            storedFile.DeletedAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Soft deleted metadata for file: {RelativePath}", relativePath);
        }
        else
        {
            _logger.LogWarning("Metadata not found or already deleted for file: {RelativePath}", relativePath);
        }

        // According to requirement: "Mặc định sử dụng soft delete. Chỉ xóa file vật lý khi nghiệp vụ xác nhận không còn tham chiếu hoặc có job dọn rác."
        // So we do NOT delete the physical file here.
    }

    public async Task<StorageHealthResult> CheckHealthAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var absoluteRoot = Path.GetFullPath(_options.RootPath);
            var driveRoot = Path.GetPathRoot(absoluteRoot);

            if (!string.IsNullOrEmpty(driveRoot) && !Directory.Exists(driveRoot))
            {
                return new StorageHealthResult
                {
                    Status = "Unhealthy",
                    StorageAvailable = false,
                    FreeSpaceBytes = 0,
                    ErrorMessage = $"Ổ đĩa '{driveRoot}' không tồn tại hoặc chưa kết nối."
                };
            }

            // Check if root is writable
            Directory.CreateDirectory(absoluteRoot);
            
            var tempDir = Path.GetFullPath(_options.TempPath);
            Directory.CreateDirectory(tempDir);

            // Try write and delete test file
            var testFilePath = Path.Combine(tempDir, $"health_check_{Guid.NewGuid():N}.tmp");
            await File.WriteAllTextAsync(testFilePath, "test", cancellationToken);
            File.Delete(testFilePath);

            long freeSpace = 0;
            if (!string.IsNullOrEmpty(driveRoot))
            {
                var driveInfo = new DriveInfo(driveRoot);
                freeSpace = driveInfo.AvailableFreeSpace;
            }

            var isHealthy = freeSpace >= _options.MinimumFreeSpaceBytes;

            return new StorageHealthResult
            {
                Status = isHealthy ? "Healthy" : "Degraded",
                StorageAvailable = true,
                FreeSpaceBytes = freeSpace,
                ErrorMessage = isHealthy ? null : $"Dung lượng trống ({freeSpace} bytes) dưới ngưỡng tối thiểu ({_options.MinimumFreeSpaceBytes} bytes)."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Storage health check failed.");
            return new StorageHealthResult
            {
                Status = "Unhealthy",
                StorageAvailable = false,
                FreeSpaceBytes = 0,
                ErrorMessage = ex.Message
            };
        }
    }

    private async Task<StoredFileResult> SaveFileInternalAsync(
        Stream stream,
        string originalFileName,
        string contentType,
        string category,
        string extension,
        long maxSizeBytes,
        bool isPublic,
        CancellationToken cancellationToken)
    {
        // 1. Check storage availability and space
        var absoluteRoot = Path.GetFullPath(_options.RootPath);
        var driveRoot = Path.GetPathRoot(absoluteRoot);

        if (!string.IsNullOrEmpty(driveRoot) && !Directory.Exists(driveRoot))
        {
            _logger.LogError("Storage drive {DriveRoot} not connected.", driveRoot);
            throw new AppException("Dịch vụ lưu trữ không khả dụng (ổ đĩa không tồn tại).", 503);
        }

        if (!string.IsNullOrEmpty(driveRoot))
        {
            var driveInfo = new DriveInfo(driveRoot);
            if (driveInfo.AvailableFreeSpace < _options.MinimumFreeSpaceBytes)
            {
                _logger.LogError("Drive space is below threshold: {FreeSpace} bytes.", driveInfo.AvailableFreeSpace);
                throw new AppException("Dịch vụ lưu trữ không khả dụng (dung lượng ổ đĩa không đủ).", 503);
            }
        }

        // 2. Ensure directory paths exist
        var tempDir = Path.GetFullPath(_options.TempPath);
        Directory.CreateDirectory(tempDir);

        var tempFilePath = Path.Combine(tempDir, $"{Guid.NewGuid():N}.tmp");

        long totalBytesRead = 0;
        byte[] hashBytes;
        
        try
        {
            // 3. Write to Temp Path
            using (var tempFileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            {
                byte[] buffer = new byte[8192];
                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    totalBytesRead += bytesRead;
                    if (totalBytesRead > maxSizeBytes)
                    {
                        throw new BusinessException($"Kích thước file vượt quá giới hạn cho phép ({maxSizeBytes / (1024L * 1024L)} MB).");
                    }
                    await tempFileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                }
                await tempFileStream.FlushAsync(cancellationToken);
            }

            // 4. Compute SHA-256 and validate magic bytes
            byte[] header = new byte[8];
            using (var tempFileReadStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
            {
                await tempFileReadStream.ReadExactlyAsync(header, 0, header.Length, cancellationToken);
                tempFileReadStream.Seek(0, SeekOrigin.Begin);

                using (var sha256 = SHA256.Create())
                {
                    hashBytes = await sha256.ComputeHashAsync(tempFileReadStream, cancellationToken);
                }
            }

            if (!ValidateMagicBytes(extension, header))
            {
                throw new BusinessException("Nội dung file không đúng với định dạng/phần mở rộng được khai báo (magic bytes check failed).");
            }
        }
        catch
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
            throw;
        }

        var sha256Hex = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

        // 5. Move from Temp to Target Directory
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var relativeFolder = Path.Combine(category, DateTime.UtcNow.ToString("yyyy/MM/dd"));
        var targetBaseDir = isPublic ? _options.PublicPath : _options.PrivatePath;
        var absoluteTargetDir = Path.GetFullPath(Path.Combine(targetBaseDir, relativeFolder));

        EnsureWithinStorageRoot(absoluteTargetDir, targetBaseDir);
        Directory.CreateDirectory(absoluteTargetDir);

        var absoluteTargetFilePath = Path.Combine(absoluteTargetDir, storedFileName);
        
        // Prevent file overwrite
        if (File.Exists(absoluteTargetFilePath))
        {
            File.Delete(tempFilePath);
            throw new BusinessException("File đã tồn tại trên hệ thống lưu trữ.");
        }

        File.Move(tempFilePath, absoluteTargetFilePath);

        // Relative path in DB: e.g., "Public/Properties/2026/06/23/file.webp"
        var relativePath = $"{(isPublic ? "Public" : "Private")}/{category}/{DateTime.UtcNow:yyyy/MM/dd}/{storedFileName}".Replace('\\', '/');

        // 6. Save Metadata to SQL Server
        var storedFile = new StoredFile
        {
            OriginalFileName = Path.GetFileName(originalFileName),
            StoredFileName = storedFileName,
            RelativePath = relativePath,
            ContentType = contentType,
            Extension = extension,
            SizeBytes = totalBytesRead,
            Sha256 = sha256Hex,
            IsPublic = isPublic,
            EntityType = category,
            UploadedByUserId = _currentUserService.UserId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        try
        {
            _dbContext.StoredFiles.Add(storedFile);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save file metadata to database. Rolling back file creation.");
            // Rollback: delete physical file
            if (File.Exists(absoluteTargetFilePath))
            {
                File.Delete(absoluteTargetFilePath);
            }
            throw;
        }

        // Build Response URL
        var url = isPublic 
            ? $"/uploads/{category}/{DateTime.UtcNow:yyyy/MM/dd}/{storedFileName}".Replace('\\', '/')
            : $"/api/v1/files/{storedFile.Id}/download";

        return new StoredFileResult
        {
            Id = storedFile.Id,
            RelativePath = relativePath,
            Url = url,
            OriginalFileName = storedFile.OriginalFileName,
            StoredFileName = storedFile.StoredFileName,
            ContentType = storedFile.ContentType,
            SizeBytes = storedFile.SizeBytes,
            Sha256 = storedFile.Sha256
        };
    }

    private void ValidateFileSecurity(string fileName, string extension)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new BusinessException("Tên file không được trống.");
        }

        if (ForbiddenExtensions.Contains(extension))
        {
            throw new BusinessException("Loại file thực thi hoặc không an toàn không được phép upload.");
        }

        if (fileName.Contains("../") || fileName.Contains("..\\"))
        {
            throw new BusinessException("Đường dẫn file không hợp lệ (path traversal detected).");
        }
    }

    private void EnsureWithinStorageRoot(string absolutePath, string absoluteRoot)
    {
        var fullPath = Path.GetFullPath(absolutePath);
        var fullRoot = Path.GetFullPath(absoluteRoot);
        if (!fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("Đường dẫn file không hợp lệ (path traversal detected).");
        }
    }

    private static bool ValidateMagicBytes(string extension, byte[] header)
    {
        if (header == null || header.Length < 4)
            return false;

        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".png" => header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47,
            ".webp" => header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46, // RIFF
            ".pdf" => header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46, // %PDF
            ".docx" or ".xlsx" => header[0] == 0x50 && header[1] == 0x4B && header[2] == 0x03 && header[3] == 0x04, // PK..
            _ => false
        };
    }
}
