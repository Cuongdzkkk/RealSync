namespace RealSync.Services.Options;

/// <summary>
/// Cấu hình đường dẫn và giới hạn cho local file storage.
/// Development: App_Data (relative to app root).
/// Production: R:\RealSyncData (set via env vars Storage__RootPath, etc.).
/// </summary>
public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    /// <summary>Thư mục gốc lưu trữ (VD: App_Data hoặc R:\RealSyncData).</summary>
    public string RootPath { get; set; } = string.Empty;

    /// <summary>Thư mục lưu file công khai (serve qua static files).</summary>
    public string PublicPath { get; set; } = string.Empty;

    /// <summary>Thư mục lưu file riêng tư (serve qua controller).</summary>
    public string PrivatePath { get; set; } = string.Empty;

    /// <summary>Thư mục tạm cho safe write workflow.</summary>
    public string TempPath { get; set; } = string.Empty;

    /// <summary>Dung lượng trống tối thiểu (bytes). Default 5 GB.</summary>
    public long MinimumFreeSpaceBytes { get; set; } =
        5L * 1024 * 1024 * 1024;

    /// <summary>Kích thước tối đa cho ảnh public (MB). Default 15.</summary>
    public int MaxPublicImageSizeMb { get; set; } = 15;

    /// <summary>Kích thước tối đa cho tài liệu private (MB). Default 50.</summary>
    public int MaxPrivateDocumentSizeMb { get; set; } = 50;
}
