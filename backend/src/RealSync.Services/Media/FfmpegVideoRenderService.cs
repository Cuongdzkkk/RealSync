using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RealSync.Core.Interfaces.Media;
using RealSync.Services.Options;
using RealSync.Shared.Exceptions;

namespace RealSync.Services.Media;

public class FfmpegVideoRenderService : IVideoRenderService
{
    private readonly VideoOptions _videoOptions;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<FfmpegVideoRenderService> _logger;

    public FfmpegVideoRenderService(
        IOptions<VideoOptions> videoOptions,
        IHostEnvironment environment,
        ILogger<FfmpegVideoRenderService> logger)
    {
        _videoOptions = videoOptions.Value;
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> ConcatenateAndOverlayAsync(
        List<string> sceneVideoPaths,
        List<SceneOverlayText> overlays,
        string outputDirectory,
        CancellationToken cancellationToken)
    {
        if (sceneVideoPaths == null || sceneVideoPaths.Count == 0)
        {
            throw new BusinessException("Danh sách phân cảnh rỗng, không thể ghép video.");
        }

        Directory.CreateDirectory(outputDirectory);
        var outputFileName = $"render_{Guid.NewGuid():N}.mp4";
        var outputFilePath = Path.Combine(outputDirectory, outputFileName);

        // Check if FFmpeg is available
        var ffmpegAvailable = IsFfmpegAvailable();
        if (!ffmpegAvailable)
        {
            if (_environment.IsDevelopment())
            {
                _logger.LogWarning("FFmpeg not found in path. Falling back to simulated rendering in Development (copying first scene).");
                if (!File.Exists(sceneVideoPaths[0]))
                {
                    throw new BusinessException($"Không tìm thấy file phân cảnh gốc: {sceneVideoPaths[0]}");
                }
                File.Copy(sceneVideoPaths[0], outputFilePath, overwrite: true);
                return outputFilePath;
            }
            throw new BusinessException("Không tìm thấy trình xử lý video FFmpeg trên hệ thống. Vui lòng liên hệ quản trị viên.");
        }

        // Real FFmpeg execution using safe arguments
        // 1. Create a temporary text file listing the input video clips for the concat demuxer
        var concatListPath = Path.Combine(_videoOptions.TempDirectory, $"concat_{Guid.NewGuid():N}.txt");
        Directory.CreateDirectory(_videoOptions.TempDirectory);

        try
        {
            var sb = new StringBuilder();
            foreach (var path in sceneVideoPaths)
            {
                // Normalize file path for FFmpeg concat format
                var fullPath = Path.GetFullPath(path).Replace("\\", "/");
                sb.AppendLine($"file '{fullPath}'");
            }
            await File.WriteAllTextAsync(concatListPath, sb.ToString(), cancellationToken);

            // 2. Perform concatenation using FFmpeg process
            // We concatenate without re-encoding first to be fast, but if drawtext is needed, we'll re-encode
            // For now, let's do a fast concat: ffmpeg -y -f concat -safe 0 -i concatList -c copy output
            var args = new List<string>
            {
                "-y",
                "-f", "concat",
                "-safe", "0",
                "-i", concatListPath
            };

            // If we have overlays, we will re-encode using the drawtext filter
            var filterString = BuildDrawtextFilters(overlays);
            if (!string.IsNullOrEmpty(filterString))
            {
                args.Add("-vf");
                args.Add(filterString);
                args.Add("-c:v");
                args.Add("libx264");
                args.Add("-preset");
                args.Add("fast");
                args.Add("-crf");
                args.Add("23");
            }
            else
            {
                args.Add("-c");
                args.Add("copy");
            }

            args.Add(outputFilePath);

            _logger.LogInformation("Invoking FFmpeg: Path={Path}, Args={Args}", 
                _videoOptions.FfmpegPath, string.Join(" ", args));

            var success = await RunProcessAsync(_videoOptions.FfmpegPath, args, cancellationToken);
            if (!success)
            {
                throw new BusinessException("Tiến trình FFmpeg không hoàn tất thành công hoặc bị timeout.");
            }

            return outputFilePath;
        }
        finally
        {
            if (File.Exists(concatListPath))
            {
                try { File.Delete(concatListPath); } catch { /* ignore */ }
            }
        }
    }

    private bool IsFfmpegAvailable()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = _videoOptions.FfmpegPath,
                Arguments = "-version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi);
            if (process == null) return false;
            process.WaitForExit(1000);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private static string BuildDrawtextFilters(List<SceneOverlayText> overlays)
    {
        if (overlays == null || overlays.Count == 0) return string.Empty;

        // Try standard Windows Font path or fall back
        var fontPath = "C:/Windows/Fonts/arial.ttf";
        if (!File.Exists(fontPath))
        {
            fontPath = "arial.ttf"; // Hope FFmpeg finds it in working directory or default PATH
        }

        var filters = new List<string>();
        foreach (var overlay in overlays)
        {
            var text = EscapeDrawtextText(overlay.Text);
            var startTime = overlay.StartTimeSeconds;
            var endTime = startTime + overlay.DurationSeconds;

            // drawtext filter: box=1 for background border to make text legible, centered at x, near bottom at y
            filters.Add($"drawtext=fontfile='{fontPath}':text='{text}':fontcolor=white:fontsize=28:box=1:boxcolor=black@0.6:boxborderw=10:x=(w-text_w)/2:y=h-120:enable='between(t,{startTime},{endTime})'");
        }

        return string.Join(",", filters);
    }

    private static string EscapeDrawtextText(string text)
    {
        // drawtext text needs quotes or escape characters for colon, backslash, single quote, etc.
        return text
            .Replace("\\", "\\\\")
            .Replace(":", "\\:")
            .Replace("'", "'\\\\''");
    }

    private async Task<bool> RunProcessAsync(string fileName, List<string> arguments, CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var arg in arguments)
        {
            psi.ArgumentList.Add(arg);
        }

        using var process = new Process { StartInfo = psi };
        
        var tcs = new TaskCompletionSource<bool>();
        process.EnableRaisingEvents = true;
        process.Exited += (sender, args) => tcs.TrySetResult(process.ExitCode == 0);

        _logger.LogInformation("Starting external process: {FileName}", fileName);
        if (!process.Start())
        {
            return false;
        }

        // Set timeout to 120 seconds
        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(120), cancellationToken);
        var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

        if (completedTask == timeoutTask)
        {
            _logger.LogWarning("Process {FileName} timed out after 120s. Killing process...", fileName);
            try
            {
                process.Kill();
            }
            catch { /* ignore */ }
            return false;
        }

        return await tcs.Task;
    }
}
