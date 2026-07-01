using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RealSync.Core.Interfaces.Media;

/// <summary>
/// Dịch vụ xử lý ghép nối video và tạo hiệu ứng văn bản bằng FFmpeg.
/// </summary>
public interface IVideoRenderService
{
    /// <summary>
    /// Ghép nối danh sách file video cục bộ và đè chữ lên từng đoạn.
    /// </summary>
    Task<string> ConcatenateAndOverlayAsync(
        List<string> sceneVideoPaths,
        List<SceneOverlayText> overlays,
        string outputDirectory,
        CancellationToken cancellationToken);
}

public class SceneOverlayText
{
    public int StartTimeSeconds { get; set; }
    public int DurationSeconds { get; set; }
    public string Text { get; set; } = string.Empty;
}
