using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RealSync.Core.Entities;
using RealSync.Core.Interfaces;
using RealSync.Data.Context;
using RealSync.Services.Options;
using RealSync.Shared.DTOs.Requests.Posts;
using RealSync.Shared.DTOs.Responses.Posts;
using RealSync.Shared.Exceptions;

namespace RealSync.Services.Implementations;

/// <summary>
/// Service tạo nội dung bài đăng bằng AI.
/// Ưu tiên: Gemini API key → opencode CLI (local, free) → Mock fallback.
/// </summary>
public class AIContentService : IAIContentService
{
    private static readonly string ProjectRoot = FindProjectRoot();
    private static bool? _isOpencodeAvailable;
    private static readonly object _lock = new();

    private readonly RealSyncDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly HttpClient _httpClient;
    private readonly AIOptions _aiOptions;

    public AIContentService(
        RealSyncDbContext context,
        ICurrentUserService currentUser,
        HttpClient httpClient,
        IOptions<AIOptions> aiOptions)
    {
        _context = context;
        _currentUser = currentUser;
        _httpClient = httpClient;
        _aiOptions = aiOptions.Value;
    }

    public async Task<AIContentGenerationResponse> GenerateAsync(
        Guid postId, AIContentGenerateRequest request)
    {
        var post = await _context.Posts
            .Include(p => p.Property)
            .FirstOrDefaultAsync(p => p.Id == postId)
            ?? throw new NotFoundException("Post", postId);

        if (string.IsNullOrWhiteSpace(request.Prompt))
            throw new BusinessException("Prompt không được để trống.");

        // Ưu tiên: Gemini key → opencode CLI (local, free) → Mock
        string generatedContent;
        if (_aiOptions.IsConfigured)
        {
            generatedContent = await CallGeminiAsync(request.Prompt, post);
        }
        else if (await IsOpencodeAvailableAsync())
        {
            generatedContent = await CallOpencodeAsync(request.Prompt, post);
        }
        else
        {
            generatedContent = GenerateMockContent(request.Prompt, post);
        }

        var generation = new AIContentGeneration
        {
            PostId = postId,
            Prompt = request.Prompt,
            GeneratedContent = generatedContent,
            CreatedBy = _currentUser.Email,
        };

        _context.AIContentGenerations.Add(generation);
        await _context.SaveChangesAsync();

        return MapToResponse(generation);
    }

    // ============================================================
    //  Gemini API
    // ============================================================

    private async Task<string> CallGeminiAsync(string prompt, Post? post = null)
    {
        var propertyInfo = (post != null && post.Property != null)
            ? $"\n\nThông tin BĐS: {post.Property.Title} - {post.Property.Description ?? ""}"
            : "";

        var fullPrompt = $"{prompt}{propertyInfo}\n\nViết bằng tiếng Việt, giọng văn chuyên nghiệp, hấp dẫn.";

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_aiOptions.Model}:generateContent?key={_aiOptions.ApiKey}";

        var body = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = fullPrompt }
                    }
                }
            },
            generationConfig = new
            {
                maxOutputTokens = _aiOptions.MaxTokens,
                temperature = _aiOptions.Temperature,
            }
        };

        var response = await _httpClient.PostAsJsonAsync(url, body);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var text = json
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return text ?? "⚠️ Gemini không trả về nội dung.";
    }

    // ============================================================
    //  opencode CLI (local, free, không cần API key)
    // ============================================================

    /// <summary>Kiểm tra opencode có sẵn trên máy không</summary>
    private static async Task<bool> IsOpencodeAvailableAsync()
    {
        if (_isOpencodeAvailable.HasValue && _isOpencodeAvailable.Value)
            return true;

        return await Task.Run(() =>
        {
            lock (_lock)
            {
                if (_isOpencodeAvailable.HasValue && _isOpencodeAvailable.Value)
                    return true;

                try
                {
                    var opencodePath = GetOpencodeExecutablePath();
                    using var proc = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = opencodePath ?? "cmd.exe",
                            Arguments = opencodePath != null ? "--version" : "/c opencode --version",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                        }
                    };
                    proc.Start();
                    proc.WaitForExit(5000);
                    _isOpencodeAvailable = proc.ExitCode == 0;
                }
                catch
                {
                    _isOpencodeAvailable = false;
                }
                return _isOpencodeAvailable.Value;
            }
        });
    }

    /// <summary>Gọi opencode CLI để sinh nội dung (free, local)</summary>
    private static async Task<string> CallOpencodeAsync(string prompt, Post? post = null)
    {
        var propertyInfo = (post != null && post.Property != null)
            ? $"\n\nThông tin BĐS: {post.Property.Title}"
            : "";

        var fullPrompt = $"{prompt}{propertyInfo}\n\nViết bằng tiếng Việt, giọng văn chuyên nghiệp.";

        var opencodePath = GetOpencodeExecutablePath();
        using var proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = opencodePath ?? "cmd.exe",
                Arguments = opencodePath != null
                    ? $"run --dangerously-skip-permissions --pure --model opencode/deepseek-v4-flash-free --format json \"{EscapeArg(fullPrompt)}\""
                    : $"/c opencode run --dangerously-skip-permissions --pure --model opencode/deepseek-v4-flash-free --format json \"{EscapeArg(fullPrompt)}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = ProjectRoot,
            }
        };

        var outputBuilder = new System.Text.StringBuilder();
        var errorBuilder = new System.Text.StringBuilder();

        proc.Start();

        // Đọc NDJSON từ stdout
        var outputTask = Task.Run(async () =>
        {
            using var reader = proc.StandardOutput;
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                try
                {
                    using var doc = JsonDocument.Parse(line);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("type", out var typeEl) && typeEl.GetString() == "text")
                    {
                        var text = root.GetProperty("part").GetProperty("text").GetString();
                        if (!string.IsNullOrEmpty(text))
                            outputBuilder.Append(text);
                    }
                }
                catch { /* skip dòng không parse được */ }
            }
        });

        var errorTask = proc.StandardError.ReadToEndAsync();

        // Timeout 60s
        var exited = proc.WaitForExit(60_000);
        if (!exited)
        {
            proc.Kill();
            throw new TimeoutException("opencode không phản hồi trong 60s.");
        }

        await outputTask;
        var result = outputBuilder.ToString().Trim();

        if (string.IsNullOrEmpty(result))
        {
            var err = await errorTask;
            throw new Exception($"opencode không trả về nội dung. Lỗi: {err}");
        }

        return result;
    }

    private static string EscapeArg(string arg)
        => arg.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private static string? GetOpencodeExecutablePath()
    {
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrEmpty(pathEnv))
        {
            var paths = pathEnv.Split(Path.PathSeparator);
            foreach (var path in paths)
            {
                var cmdPath = Path.Combine(path, "opencode.cmd");
                if (File.Exists(cmdPath))
                    return cmdPath;

                var batPath = Path.Combine(path, "opencode.bat");
                if (File.Exists(batPath))
                    return batPath;

                var exePath = Path.Combine(path, "opencode.exe");
                if (File.Exists(exePath))
                    return exePath;
            }
        }

        var appData = Environment.GetEnvironmentVariable("APPDATA");
        if (!string.IsNullOrEmpty(appData))
        {
            var npmPath = Path.Combine(appData, "npm", "opencode.cmd");
            if (File.Exists(npmPath))
                return npmPath;
        }

        return null;
    }

    /// <summary>Tự động tìm thư mục gốc dự án (nơi có opencode.json)</summary>
    private static string FindProjectRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "opencode.json")))
                return dir.FullName;
            dir = dir.Parent;
        }
        return Directory.GetCurrentDirectory();
    }

    public async Task<IEnumerable<AIContentGenerationResponse>> GetHistoryAsync(Guid postId)
    {
        var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
        if (!postExists)
            throw new NotFoundException("Post", postId);

        var generations = await _context.AIContentGenerations
            .Where(g => g.PostId == postId)
            .AsNoTracking()
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

        return generations.Select(MapToResponse);
    }

    public async Task<AIContentGenerationResponse> GetByIdAsync(Guid postId, Guid id)
    {
        var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
        if (!postExists)
            throw new NotFoundException("Post", postId);

        var generation = await _context.AIContentGenerations
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id && g.PostId == postId)
            ?? throw new NotFoundException("AIContentGeneration", id);

        return MapToResponse(generation);
    }

    /// <summary>
    /// Tạo nội dung mẫu — sẽ thay bằng AI module thực.
    /// </summary>
    private static string GenerateMockContent(string prompt, Post post)
    {
        var propertyInfo = post.Property != null
            ? $"\n\nThông tin BĐS: {post.Property.Title}"
            : "";

        return $"[AI Generated Content]\n\n"
            + $"Bài đăng: {post.Title}\n"
            + $"Prompt: {prompt}{propertyInfo}\n\n"
            + "Nội dung được tạo bởi AI Content Engine. "
            + "Vui lòng chỉnh sửa trước khi đăng.";
    }

    private static AIContentGenerationResponse MapToResponse(AIContentGeneration generation)
    {
        return new AIContentGenerationResponse
        {
            Id = generation.Id,
            PostId = generation.PostId,
            Prompt = generation.Prompt,
            GeneratedContent = generation.GeneratedContent,
            CreatedAt = generation.CreatedAt,
        };
    }

    public async Task<string> ChatAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new BusinessException("Tin nhắn không được để trống.");

        var systemInstructions = "Bạn là RealSync AI Copilot, trợ lý AI thông minh chuyên nghiệp tích hợp trong hệ thống CRM Bất động sản RealSync. "
            + "Nhiệm vụ của bạn là hỗ trợ người dùng (nhân viên môi giới, chủ tịch, quản lý BĐS) về chấm điểm lead, soạn tin đăng BĐS, phân tích thị trường hoặc gợi ý chăm sóc khách hàng. "
            + "Hãy trả lời bằng tiếng Việt, súc tích, chuyên nghiệp và hữu ích. Trả lời trực tiếp câu hỏi của người dùng.";

        var fullPrompt = $"{systemInstructions}\n\nNgười dùng hỏi: {message}\n\nTrả lời:";

        if (_aiOptions.IsConfigured)
        {
            return await CallGeminiAsync(fullPrompt, null);
        }
        else if (await IsOpencodeAvailableAsync())
        {
            return await CallOpencodeAsync(fullPrompt, null);
        }
        else
        {
            return GetMockChatResponse(message);
        }
    }

    private static string GetMockChatResponse(string message)
    {
        var lowercase = message.ToLower();
        if (lowercase.Contains("chấm điểm") || lowercase.Contains("score") || lowercase.Contains("lead"))
        {
            return "Hệ thống AI vừa chấm điểm khách hàng **Nguyễn Văn A** đạt **92/100 điểm**. Điểm số dựa trên hành vi điền form nhanh, tương tác 3 lần trên Zalo và tìm kiếm phân khúc **Căn hộ Quận 2**. Khuyến nghị: Liên hệ ngay trong hôm nay.";
        }
        if (lowercase.Contains("soạn") || lowercase.Contains("tin đăng") || lowercase.Contains("content"))
        {
            return "Dưới đây là tin đăng mẫu do tôi soạn thảo:\n\n**[CĂN HỘ CAO CẤP QUẬN 2 - GIÁ CHỈ 4.5 TỶ]**\n- Vị trí vàng trung tâm Thạnh Mỹ Lợi.\n- Diện tích 75m², 2 phòng ngủ rộng rãi, ban công view sông Sài Gòn thoáng mát.\n- Nội thất bàn giao cao cấp, liền kề tuyến Metro.\n📞 Liên hệ ngay hôm nay để nhận thông tin chiết khấu!";
        }
        if (lowercase.Contains("gợi ý") || lowercase.Contains("chăm sóc"))
        {
            return "Gợi ý hành động tiếp theo cho khách hàng **Trần Thị B**:\n1. Gửi tài liệu phân tích mặt bằng căn hộ qua Zalo.\n2. Lên lịch hẹn xem nhà vào lúc 9:00 sáng Thứ Bảy tuần này.\n3. Cập nhật ghi chú trên CRM.";
        }
        return "Tôi đã ghi nhận yêu cầu của bạn. Tuy nhiên hiện tại kết nối AI cục bộ (opencode) hoặc Gemini API chưa được mở trên máy chủ. Đây là câu trả lời giả lập từ hệ thống RealSync.";
    }
}
