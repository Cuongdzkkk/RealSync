using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RealSync.Core.Entities;
using RealSync.Core.Interfaces;
using RealSync.Data.Context;
using RealSync.Shared.DTOs.Requests.Posts;
using RealSync.Shared.DTOs.Responses.Posts;
using RealSync.Shared.Exceptions;

namespace RealSync.Services.Implementations;

/// <summary>
/// Service quản lý kênh đăng bài cho Post.
/// </summary>
public class PostChannelService : IPostChannelService
{
    private static readonly string ProjectRoot = FindProjectRoot();
    private readonly RealSyncDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IConfiguration _configuration;

    public PostChannelService(
        RealSyncDbContext context, 
        ICurrentUserService currentUser, 
        IConfiguration configuration)
    {
        _context = context;
        _currentUser = currentUser;
        _configuration = configuration;
    }

    public async Task<IEnumerable<PostChannelResponse>> GetByPostIdAsync(Guid postId)
    {
        await EnsurePostExistsAsync(postId);

        var channels = await _context.PostChannels
            .Where(c => c.PostId == postId)
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return channels.Select(MapToResponse);
    }

    public async Task<PostChannelResponse> CreateAsync(Guid postId, PostChannelCreateRequest request)
    {
        await EnsurePostExistsAsync(postId);

        // Kiểm tra channel đã tồn tại cho post này chưa
        var exists = await _context.PostChannels
            .AnyAsync(c => c.PostId == postId && c.Channel == request.Channel);
        if (exists)
            throw new BusinessException($"Kênh '{request.Channel}' đã được thêm cho bài đăng này.");

        var channel = new PostChannel
        {
            PostId = postId,
            Channel = request.Channel,
            PublishStatus = "Pending",
            CreatedBy = _currentUser.Email,
        };

        _context.PostChannels.Add(channel);
        await _context.SaveChangesAsync();

        return MapToResponse(channel);
    }

    public async Task<PostChannelResponse> UpdateAsync(
        Guid postId, Guid id, PostChannelUpdateRequest request)
    {
        var channel = await GetChannelAsync(postId, id);

        channel.Channel = request.Channel;
        channel.UpdatedAt = DateTime.UtcNow;
        channel.UpdatedBy = _currentUser.Email;

        await _context.SaveChangesAsync();

        return MapToResponse(channel);
    }

    public async Task DeleteAsync(Guid postId, Guid id)
    {
        var channel = await GetChannelAsync(postId, id);

        channel.IsDeleted = true;
        channel.DeletedAt = DateTime.UtcNow;
        channel.UpdatedBy = _currentUser.Email;

        await _context.SaveChangesAsync();
    }

    public async Task<PostChannelResponse> PublishAsync(Guid postId, Guid id)
    {
        var channel = await GetChannelAsync(postId, id);

        var contentText = channel.Post.Content;
        if (string.IsNullOrWhiteSpace(contentText))
        {
            // Fallback to the latest AIContentGeneration
            var latestAiGen = await _context.AIContentGenerations
                .Where(g => g.PostId == postId)
                .OrderByDescending(g => g.CreatedAt)
                .FirstOrDefaultAsync();
            if (latestAiGen != null)
            {
                contentText = latestAiGen.GeneratedContent;
            }
        }

        if (string.IsNullOrWhiteSpace(contentText))
        {
            throw new BusinessException("Nội dung bài đăng trống, không thể thực hiện publish.");
        }

        string channelName = channel.Channel.ToLower();
        try
        {
            if (channelName == "zalo")
            {
                var zaloGroupId = _configuration["Posting:Zalo:TargetGroupId"] ?? "g123456789";
                var zaloAgentPath = Path.Combine(ProjectRoot, "agent-skills", "zalo-agent-cli", "src", "index.js");

                if (!File.Exists(zaloAgentPath))
                {
                    throw new FileNotFoundException($"Không tìm thấy zalo-agent CLI tại: {zaloAgentPath}");
                }

                var isGroup = zaloGroupId.StartsWith("g", StringComparison.OrdinalIgnoreCase);
                var arguments = $"\"{zaloAgentPath}\" msg send {zaloGroupId} \"{EscapeArg(contentText)}\"{(isGroup ? " -t 1" : "")}";

                using var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "node",
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WorkingDirectory = Path.Combine(ProjectRoot, "agent-skills", "zalo-agent-cli")
                    }
                };

                proc.Start();
                var outputTask = proc.StandardOutput.ReadToEndAsync();
                var errorTask = proc.StandardError.ReadToEndAsync();

                if (!proc.WaitForExit(45_000))
                {
                    proc.Kill();
                    throw new TimeoutException("zalo-agent CLI không phản hồi trong 45 giây.");
                }

                if (proc.ExitCode != 0)
                {
                    var errorMsg = await errorTask;
                    throw new Exception($"Lỗi thực thi zalo-agent: {errorMsg}");
                }

                channel.PublishStatus = "Published";
                channel.PublishedAt = DateTime.UtcNow;
            }
            else if (channelName == "facebook")
            {
                var publishFbPath = Path.Combine(ProjectRoot, "agent-skills", "social-auto-engine", "publish_fb.py");

                if (!File.Exists(publishFbPath))
                {
                    throw new FileNotFoundException($"Không tìm thấy script publish Facebook tại: {publishFbPath}");
                }

                var arguments = $"\"{publishFbPath}\" \"{EscapeArg(contentText)}\" \"{EscapeArg(channel.Post.ThumbnailUrl ?? "")}\"";

                using var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "python",
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WorkingDirectory = Path.Combine(ProjectRoot, "agent-skills", "social-auto-engine")
                    }
                };

                proc.Start();
                var outputTask = proc.StandardOutput.ReadToEndAsync();
                var errorTask = proc.StandardError.ReadToEndAsync();

                if (!proc.WaitForExit(45_000))
                {
                    proc.Kill();
                    throw new TimeoutException("Social Auto Engine (Facebook) không phản hồi trong 45 giây.");
                }

                var output = await outputTask;
                if (proc.ExitCode != 0)
                {
                    var errorMsg = await errorTask;
                    throw new Exception($"Lỗi thực thi script post Facebook: {errorMsg}");
                }

                channel.PublishStatus = "Published";
                channel.PublishedAt = DateTime.UtcNow;

                try
                {
                    using var doc = JsonDocument.Parse(output);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("id", out var idEl))
                    {
                        channel.PublishedUrl = $"https://facebook.com/{idEl.GetString()}";
                    }
                }
                catch { /* bypass parse error */ }
            }
            else
            {
                // Fallback cho các kênh khác (Website, SEO...)
                channel.PublishStatus = "Published";
                channel.PublishedAt = DateTime.UtcNow;
            }
        }
        catch (Exception ex)
        {
            channel.PublishStatus = "Failed";
            channel.ErrorMessage = ex.Message;
            await _context.SaveChangesAsync();
            throw new BusinessException($"Lỗi đăng bài tự động lên {channel.Channel}: {ex.Message}");
        }

        channel.UpdatedAt = DateTime.UtcNow;
        channel.UpdatedBy = _currentUser.Email;

        await _context.SaveChangesAsync();

        return MapToResponse(channel);
    }

    private static string EscapeArg(string arg)
        => arg.Replace("\\", "\\\\").Replace("\"", "\\\"");

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

    private async Task EnsurePostExistsAsync(Guid postId)
    {
        var exists = await _context.Posts.AnyAsync(p => p.Id == postId);
        if (!exists)
            throw new NotFoundException("Post", postId);
    }

    private async Task<PostChannel> GetChannelAsync(Guid postId, Guid id)
    {
        await EnsurePostExistsAsync(postId);

        return await _context.PostChannels
            .Include(c => c.Post)
            .FirstOrDefaultAsync(c => c.Id == id && c.PostId == postId)
            ?? throw new NotFoundException("PostChannel", id);
    }

    private static PostChannelResponse MapToResponse(PostChannel channel)
    {
        return new PostChannelResponse
        {
            Id = channel.Id,
            PostId = channel.PostId,
            Channel = channel.Channel,
            PublishStatus = channel.PublishStatus,
            PublishedUrl = channel.PublishedUrl,
            PublishedAt = channel.PublishedAt,
            ErrorMessage = channel.ErrorMessage,
            CreatedAt = channel.CreatedAt,
        };
    }
}
