using RealSync.Shared.DTOs.Requests;

namespace RealSync.Core.Interfaces;

public interface IFileStorageService
{
    Task<FileStorageResult> UploadAsync(string key, FileUploadRequest file, CancellationToken cancellationToken = default);
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
}

public class FileStorageResult
{
    public string Key { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
