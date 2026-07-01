using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RealSync.Core.Entities;
using RealSync.Core.Interfaces;
using RealSync.Core.Interfaces.Publishing;
using RealSync.Data.Context;
using RealSync.Services.Options;
using RealSync.Shared.DTOs.Requests.Posts;
using RealSync.Shared.DTOs.Responses.Posts;
using Microsoft.Extensions.Logging;
using RealSync.Shared.Exceptions;

namespace RealSync.Services.Implementations;

/// <summary>
/// Service tạo nội dung bài đăng bằng AI.
/// </summary>
public class AIContentService : IAIContentService
{
    private static readonly string ProjectRoot = FindProjectRoot();
    private static readonly string PlatformConfigPath = Path.Combine(ProjectRoot, "platform_config.json");

    private readonly RealSyncDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IAITextProvider _aiProvider;
    private readonly AIOptions _aiOptions;
    private readonly ILogger<AIContentService> _logger;

    public AIContentService(
        RealSyncDbContext context,
        ICurrentUserService currentUser,
        IAITextProvider aiProvider,
        IOptions<AIOptions> aiOptions,
        ILogger<AIContentService> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _aiProvider = aiProvider;
        _aiOptions = aiOptions.Value;
        _logger = logger;
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

        if (!_aiOptions.IsConfigured || string.IsNullOrWhiteSpace(_aiOptions.ApiKey))
        {
            throw new BusinessException("Gemini API Key chưa được cấu hình. Vui lòng thiết lập trong Cài đặt hệ thống.");
        }



        StructuredAIOutputResult providerResult;
        string generatedContent;
        int promptTokens;
        int completionTokens;
        decimal estimatedCost;
        string factsUsedJson;

        try
        {
            var propertyData = post.Property != null ? JsonSerializer.Serialize(new
            {
                title = post.Property.Title,
                description = post.Property.Description,
                price = post.Property.Price,
                bedrooms = post.Property.Bedrooms,
                bathrooms = post.Property.Bathrooms,
                address = post.Property.Address
            }) : null;

            var providerRequest = new AITextRequest
            {
                Prompt = BuildFullPrompt(request.Prompt, post),
                Model = _aiOptions.Model,
                Temperature = _aiOptions.Temperature,
                MaxTokens = _aiOptions.MaxTokens,
                OriginalDataJson = propertyData
            };

            providerResult = await _aiProvider.GenerateStructuredAsync(providerRequest, CancellationToken.None);

            var output = providerResult.Output;
            var fullText = "";
            if (!string.IsNullOrEmpty(output.Title)) fullText += $"**{output.Title}**\n\n";
            if (!string.IsNullOrEmpty(output.Summary)) fullText += $"{output.Summary}\n\n";
            if (!string.IsNullOrEmpty(output.Caption)) fullText += $"{output.Caption}\n\n";
            if (output.Hashtags != null && output.Hashtags.Count > 0)
                fullText += string.Join(" ", output.Hashtags.Select(h => h.StartsWith("#") ? h : "#" + h)) + "\n\n";
            if (!string.IsNullOrEmpty(output.CallToAction)) fullText += $"📞 {output.CallToAction}";

            generatedContent = fullText.Trim();
            promptTokens = providerResult.PromptTokens;
            completionTokens = providerResult.CompletionTokens;
            estimatedCost = (promptTokens * 0.000000075m) + (completionTokens * 0.0000003m);

            var trackingObj = new
            {
                factsUsed = output.FactsUsed,
                warnings = output.Warnings
            };
            factsUsedJson = JsonSerializer.Serialize(trackingObj);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini generation failed.");
            throw new BusinessException($"Không thể tạo nội dung bằng AI. Lỗi: {ex.Message}");
        }

        var generation = new AIContentGeneration
        {
            PostId = postId,
            Prompt = request.Prompt,
            GeneratedContent = generatedContent,
            CreatedBy = _currentUser.Email,
            PromptTokens = promptTokens,
            CompletionTokens = completionTokens,
            EstimatedCost = estimatedCost,
            FactsUsedJson = factsUsedJson
        };

        _context.AIContentGenerations.Add(generation);

        // Auto-save the generated content directly to the parent Post entity to prevent null content issues
        post.Content = generatedContent;
        if (string.IsNullOrWhiteSpace(post.Summary) || post.Summary.Contains("Kênh:"))
        {
            post.Summary = generatedContent.Length > 200 
                ? generatedContent.Substring(0, 200) + "..." 
                : generatedContent;
        }

        await _context.SaveChangesAsync();

        return MapToResponse(generation);
    }

    private static string BuildFullPrompt(string prompt, Post post)
    {
        var propertyInfo = (post != null && post.Property != null)
            ? $"\n\nThông tin BĐS gốc để đối chiếu: Tiêu đề={post.Property.Title}, Mô tả={post.Property.Description ?? ""}, Giá={post.Property.Price}, Phòng ngủ={post.Property.Bedrooms}, Phòng tắm={post.Property.Bathrooms}, Địa chỉ={post.Property.Address}"
            : "";

        string fullPrompt;
        if (prompt.Contains("trích xuất thông tin dưới dạng JSON") || prompt.Contains("JSON phải có định dạng chính xác"))
        {
            fullPrompt = $"{prompt}{propertyInfo}";
        }
        else
        {
            fullPrompt = $"{prompt}{propertyInfo}\n\nViết bằng tiếng Việt. Hãy trích xuất và đối chiếu chính xác các dữ kiện (factsUsed) như giá, diện tích, địa chỉ để đối chiếu với thông tin gốc.";
            fullPrompt = ApplyPlatformTemplateInstructions(fullPrompt);
        }
        return fullPrompt;
    }

    private static string ApplyPlatformTemplateInstructions(string prompt)
    {
        if (File.Exists(PlatformConfigPath))
        {
            try
            {
                var content = File.ReadAllText(PlatformConfigPath);
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;
                if (root.TryGetProperty("usePlatformTemplate", out var useTemplateEl) && useTemplateEl.GetBoolean() &&
                    root.TryGetProperty("templateContent", out var templateEl))
                {
                    var templateContent = templateEl.GetString();
                    if (!string.IsNullOrWhiteSpace(templateContent))
                    {
                        return $"{prompt}\n\n[YÊU CẦU ĐẶC BIỆT - MẪU CẤU TRÚC THƯƠNG HIỆU (PLATFORM TEMPLATE)]:\nHãy viết nội dung đúng và khít theo cấu trúc mẫu sau đây. Đọc mẫu này và tìm các chỗ hợp lý để thay tên đất, tên dự án, thông tin liên hệ, diện tích, giá bán vào. Giữ nguyên cấu trúc định dạng của mẫu:\n\n{templateContent}";
                    }
                }
            }
            catch { /* bypass */ }
        }
        return prompt;
    }

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

    private static AIContentGenerationResponse MapToResponse(AIContentGeneration generation)
    {
        return new AIContentGenerationResponse
        {
            Id = generation.Id,
            PostId = generation.PostId,
            Prompt = generation.Prompt,
            GeneratedContent = generation.GeneratedContent,
            CreatedAt = generation.CreatedAt,
            PromptTokens = generation.PromptTokens,
            CompletionTokens = generation.CompletionTokens,
            EstimatedCost = generation.EstimatedCost,
            FactsUsedJson = generation.FactsUsedJson
        };
    }

    public async Task ApplyAsync(Guid postId, ApplyAIContentRequest request)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == postId)
            ?? throw new NotFoundException("Post", postId);

        if (string.IsNullOrWhiteSpace(request.Content))
            throw new BusinessException("Nội dung bài viết không được rỗng khi áp dụng.");

        post.Content = request.Content;
        if (!string.IsNullOrEmpty(request.Summary))
        {
            post.Summary = request.Summary;
        }
        else if (request.Content.Length > 200)
        {
            post.Summary = request.Content.Substring(0, 200) + "...";
        }
        else
        {
            post.Summary = request.Content;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<string> ChatAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new BusinessException("Tin nhắn không được để trống.");

        string fullPrompt;
        var msgLower = message.ToLower();
        if (msgLower.Contains("intent") && msgLower.Contains("budget") && msgLower.Contains("bedrooms"))
        {
            fullPrompt = message;
        }
        else
        {
            var systemInstructions = "Bạn là RealSync AI Copilot, trợ lý AI thông minh chuyên nghiệp tích hợp trong hệ thống CRM Bất động sản RealSync. "
                + "Nhiệm vụ của bạn là hỗ trợ người dùng về chấm điểm lead, soạn tin đăng BĐS, phân tích thị trường hoặc gợi ý chăm sóc khách hàng. "
                + "Hãy trả lời bằng tiếng Việt, súc tích, chuyên nghiệp và hữu ích.";

            fullPrompt = $"{systemInstructions}\n\nNgười dùng hỏi: {message}\n\nTrả lời:";
        }

        if (!_aiOptions.IsConfigured || string.IsNullOrWhiteSpace(_aiOptions.ApiKey))
        {
            throw new BusinessException("Gemini API Key chưa được cấu hình. Vui lòng thiết lập trong Cài đặt hệ thống.");
        }



        try
        {
            return await _aiProvider.GenerateTextAsync(fullPrompt, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chat via Gemini failed.");
            throw new BusinessException($"Lỗi kết nối AI: {ex.Message}");
        }
    }
}
