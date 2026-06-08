using Microsoft.EntityFrameworkCore;
using RealSync.Core.Entities;
using RealSync.Core.Interfaces;
using RealSync.Data.Context;
using RealSync.Shared.DTOs.Requests.Posts;
using RealSync.Shared.DTOs.Responses.Posts;
using RealSync.Shared.Exceptions;

namespace RealSync.Services.Implementations;

/// <summary>
/// Service tạo nội dung bài đăng bằng AI.
/// Note: Hiện tại tạo nội dung mẫu. Tích hợp AI module thực sẽ ở giai đoạn sau.
/// </summary>
public class AIContentService : IAIContentService
{
    private readonly RealSyncDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AIContentService(RealSyncDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
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

        // TODO: Tích hợp AI module thực (Content AI Engine — M6)
        // Hiện tại tạo nội dung mẫu dựa trên prompt và property info
        var generatedContent = GenerateMockContent(request.Prompt, post);

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
}
