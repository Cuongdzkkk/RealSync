namespace RealSync.Core.Entities;

/// <summary>
/// Metadata cho file đã lưu trên server.
/// Không kế thừa BaseEntity vì dùng DateTimeOffset theo yêu cầu task.
/// Chỉ lưu đường dẫn tương đối (RelativePath), không lưu đường dẫn tuyệt đối.
/// </summary>
public sealed class StoredFile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Tên file gốc do người dùng upload.</summary>
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>Tên file vật lý do server sinh ({Guid:N}{extension}).</summary>
    public string StoredFileName { get; set; } = string.Empty;

    /// <summary>Đường dẫn tương đối, VD: Public/Properties/2026/06/23/abc...def.webp</summary>
    public string RelativePath { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;

    public long SizeBytes { get; set; }
    public string Sha256 { get; set; } = string.Empty;

    /// <summary>true = public (static files), false = private (qua controller).</summary>
    public bool IsPublic { get; set; }

    /// <summary>Loại đối tượng liên quan: Property, Project, Avatar, Contract...</summary>
    public string? EntityType { get; set; }

    /// <summary>ID đối tượng liên quan.</summary>
    public Guid? EntityId { get; set; }

    public Guid? UploadedByUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
