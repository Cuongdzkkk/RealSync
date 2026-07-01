using System.Threading;
using System.Threading.Tasks;

namespace RealSync.Core.Interfaces.Publishing;

/// <summary>
/// Định nghĩa dịch vụ tạo video bằng AI (ví dụ Google Veo).
/// </summary>
public interface IVideoGenerationProvider
{
    string ProviderName { get; }

    /// <summary>
    /// Bắt đầu tiến trình sinh video từ prompt văn bản (LRO - Long Running Operation).
    /// </summary>
    Task<VideoOperationResult> StartGenerationAsync(
        string prompt,
        string aspectRatio,
        int durationSeconds,
        CancellationToken cancellationToken);

    /// <summary>
    /// Lấy trạng thái hiện tại của LRO từ provider.
    /// </summary>
    Task<VideoOperationStatusResult> GetOperationStatusAsync(
        string operationId,
        CancellationToken cancellationToken);
}

public class VideoOperationResult
{
    public string OperationId { get; set; } = string.Empty;
    public bool Done { get; set; }
    public string? VideoUrl { get; set; }
}

public class VideoOperationStatusResult
{
    public string OperationId { get; set; } = string.Empty;
    public bool Done { get; set; }
    public string? VideoUrl { get; set; }
    public int? ProgressPercent { get; set; }
    public string? ErrorMessage { get; set; }
}
