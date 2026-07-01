namespace RealSync.Services.Options;

/// <summary>
/// Cấu hình cho dịch vụ sinh video AI (Veo) và FFmpeg.
/// </summary>
public class VideoOptions
{
    public const string SectionName = "Video";

    public string Provider { get; set; } = "Veo";
    public string Model { get; set; } = "veo-3.1-generate-preview";
    public string? ApiKey { get; set; }
    
    public string FfmpegPath { get; set; } = "ffmpeg";
    public string FfprobePath { get; set; } = "ffprobe";
    
    public string TempDirectory { get; set; } = "App_Data/Temp/Video";
    public decimal DailyBudget { get; set; } = 5.0m; // limit daily cost in USD
}
