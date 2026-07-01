namespace RealSync.Core.Interfaces;

/// <summary>
/// Interface cho dịch vụ lưu trữ file.
/// Development: lưu vào App_Data.
/// Production: lưu vào ổ R:\RealSyncData.
/// Tương lai: có thể tạo S3/R2 implementation mà không sửa nghiệp vụ.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Lưu ảnh công khai (properties, projects, avatars).
    /// File được validate extension + magic bytes + kích thước.
    /// </summary>
    Task<StoredFileResult> SavePublicImageAsync(
        Stream stream,
        string originalFileName,
        string contentType,
        string category,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lưu tài liệu riêng tư (contracts, documents).
    /// File private không được serve qua static files.
    /// </summary>
    Task<StoredFileResult> SavePrivateDocumentAsync(
        Stream stream,
        string originalFileName,
        string contentType,
        string category,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mở stream đọc file theo đường dẫn tương đối.
    /// </summary>
    Task<Stream> OpenReadAsync(
        string relativePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa file vật lý theo đường dẫn tương đối.
    /// </summary>
    Task DeleteAsync(
        string relativePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra sức khỏe storage: ổ tồn tại, ghi được, dung lượng đủ.
    /// </summary>
    Task<StorageHealthResult> CheckHealthAsync(
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Kết quả sau khi lưu file thành công.
/// </summary>
public class StoredFileResult
{
    public Guid Id { get; set; }
    public string RelativePath { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string Sha256 { get; set; } = string.Empty;
}

/// <summary>
/// Kết quả health check cho storage.
/// </summary>
public class StorageHealthResult
{
    public string Status { get; set; } = "Healthy";
    public bool StorageAvailable { get; set; }
    public long FreeSpaceBytes { get; set; }
    public string? ErrorMessage { get; set; }
}
