using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealSync.Core.Interfaces;
using RealSync.Data.Context;
using RealSync.Shared.Exceptions;
using RealSync.Shared.DTOs.Responses;

namespace RealSync.Api.Controllers;

[Authorize]
public class FilesController : BaseController
{
    private readonly IFileStorageService _fileStorageService;
    private readonly RealSyncDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<FilesController> _logger;

    public FilesController(
        IFileStorageService fileStorageService,
        RealSyncDbContext dbContext,
        ICurrentUserService currentUserService,
        ILogger<FilesController> logger)
    {
        _fileStorageService = fileStorageService;
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Upload hình ảnh công khai (properties, projects, avatars).
    /// </summary>
    [HttpPost("public-images")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadPublicImage(
        IFormFile file,
        [FromForm] string category = "properties",
        [FromForm] Guid? entityId = null,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            throw new BusinessException("Vui lòng chọn file để upload.");
        }

        var allowedCategories = new[] { "properties", "projects", "avatars" };
        if (!allowedCategories.Contains(category.ToLowerInvariant()))
        {
            throw new BusinessException("Category không hợp lệ. Chỉ chấp nhận: properties, projects, avatars.");
        }

        using var stream = file.OpenReadStream();
        var result = await _fileStorageService.SavePublicImageAsync(
            stream,
            file.FileName,
            file.ContentType,
            category.ToLowerInvariant(),
            cancellationToken);

        if (entityId.HasValue)
        {
            var storedFile = await _dbContext.StoredFiles.FindAsync(new object[] { result.Id }, cancellationToken);
            if (storedFile != null)
            {
                storedFile.EntityId = entityId.Value;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        return OkResponse(result, "Upload ảnh công khai thành công.");
    }

    /// <summary>
    /// Upload tài liệu riêng tư (contracts, documents).
    /// </summary>
    [HttpPost("private-documents")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadPrivateDocument(
        IFormFile file,
        [FromForm] string category = "documents",
        [FromForm] Guid? entityId = null,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            throw new BusinessException("Vui lòng chọn file để upload.");
        }

        var allowedCategories = new[] { "contracts", "documents" };
        if (!allowedCategories.Contains(category.ToLowerInvariant()))
        {
            throw new BusinessException("Category không hợp lệ. Chỉ chấp nhận: contracts, documents.");
        }

        using var stream = file.OpenReadStream();
        var result = await _fileStorageService.SavePrivateDocumentAsync(
            stream,
            file.FileName,
            file.ContentType,
            category.ToLowerInvariant(),
            cancellationToken);

        if (entityId.HasValue)
        {
            var storedFile = await _dbContext.StoredFiles.FindAsync(new object[] { result.Id }, cancellationToken);
            if (storedFile != null)
            {
                storedFile.EntityId = entityId.Value;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        return OkResponse(result, "Upload tài liệu riêng tư thành công.");
    }

    /// <summary>
    /// Tải file riêng tư (yêu cầu đăng nhập + quyền truy cập).
    /// </summary>
    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadPrivateFile(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var storedFile = await _dbContext.StoredFiles.FindAsync(new object[] { id }, cancellationToken);
        if (storedFile == null || storedFile.IsDeleted)
        {
            throw new NotFoundException("File", id);
        }

        // Kiểm tra quyền truy cập đối với file private
        if (!storedFile.IsPublic)
        {
            var currentUserId = _currentUserService.UserId;
            var userRole = _currentUserService.Role;

            if (userRole != "Admin" && storedFile.UploadedByUserId != currentUserId)
            {
                throw new ForbiddenException("Bạn không có quyền tải file này.");
            }
        }

        var stream = await _fileStorageService.OpenReadAsync(storedFile.RelativePath, cancellationToken);
        return File(stream, storedFile.ContentType, storedFile.OriginalFileName);
    }

    /// <summary>
    /// Lấy thông tin metadata của file.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetMetadata(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var storedFile = await _dbContext.StoredFiles.FindAsync(new object[] { id }, cancellationToken);
        if (storedFile == null || storedFile.IsDeleted)
        {
            throw new NotFoundException("File", id);
        }

        // Kiểm tra quyền xem metadata đối với file private
        if (!storedFile.IsPublic)
        {
            var currentUserId = _currentUserService.UserId;
            var userRole = _currentUserService.Role;

            if (userRole != "Admin" && storedFile.UploadedByUserId != currentUserId)
            {
                throw new ForbiddenException("Bạn không có quyền xem thông tin file này.");
            }
        }

        // Build Response URL
        var url = storedFile.IsPublic 
            ? $"/uploads/{storedFile.EntityType}/{storedFile.CreatedAt:yyyy/MM/dd}/{storedFile.StoredFileName}".Replace('\\', '/')
            : $"/api/v1/files/{storedFile.Id}/download";

        var response = new StoredFileResult
        {
            Id = storedFile.Id,
            RelativePath = storedFile.RelativePath,
            Url = url,
            OriginalFileName = storedFile.OriginalFileName,
            StoredFileName = storedFile.StoredFileName,
            ContentType = storedFile.ContentType,
            SizeBytes = storedFile.SizeBytes,
            Sha256 = storedFile.Sha256
        };

        return OkResponse(response, "Lấy metadata file thành công.");
    }

    /// <summary>
    /// Xóa file (soft delete metadata).
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFile(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var storedFile = await _dbContext.StoredFiles.FindAsync(new object[] { id }, cancellationToken);
        if (storedFile == null || storedFile.IsDeleted)
        {
            throw new NotFoundException("File", id);
        }

        var currentUserId = _currentUserService.UserId;
        var userRole = _currentUserService.Role;

        // Chỉ Admin hoặc người upload mới được xóa
        if (userRole != "Admin" && storedFile.UploadedByUserId != currentUserId)
        {
            throw new ForbiddenException("Bạn không có quyền xóa file này.");
        }

        await _fileStorageService.DeleteAsync(storedFile.RelativePath, cancellationToken);
        return NoContent();
    }
}
