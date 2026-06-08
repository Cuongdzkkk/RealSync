using Microsoft.EntityFrameworkCore;
using RealSync.Core.Entities;
using RealSync.Core.Interfaces;
using RealSync.Data.Context;
using RealSync.Shared.DTOs.Requests.Posts;
using RealSync.Shared.DTOs.Responses.Posts;
using RealSync.Shared.Exceptions;

namespace RealSync.Services.Implementations;

/// <summary>
/// Service thống kê hiệu suất bài đăng.
/// </summary>
public class PostAnalyticsService : IPostAnalyticsService
{
    private readonly RealSyncDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public PostAnalyticsService(RealSyncDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PostAnalyticsResponse> GetByPostIdAsync(Guid postId)
    {
        var post = await _context.Posts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == postId)
            ?? throw new NotFoundException("Post", postId);

        var analytics = await _context.PostAnalytics
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.PostId == postId);

        // Auto-create analytics nếu chưa có
        if (analytics == null)
        {
            analytics = new PostAnalytics
            {
                PostId = postId,
                CreatedBy = _currentUser.Email,
            };
            _context.PostAnalytics.Add(analytics);
            await _context.SaveChangesAsync();
        }

        return MapToResponse(analytics, post.Title);
    }

    public async Task<PostAnalyticsSummaryResponse> GetSummaryAsync()
    {
        var totalPosts = await _context.Posts.CountAsync();
        var totalPublished = await _context.Posts
            .CountAsync(p => p.Status == "Published");

        var analyticsData = await _context.PostAnalytics
            .AsNoTracking()
            .ToListAsync();

        return new PostAnalyticsSummaryResponse
        {
            TotalPosts = totalPosts,
            TotalPublished = totalPublished,
            TotalViews = analyticsData.Sum(a => a.Views),
            TotalClicks = analyticsData.Sum(a => a.Clicks),
            TotalLeadsGenerated = analyticsData.Sum(a => a.LeadsGenerated),
            AverageConversionRate = analyticsData.Count > 0
                ? Math.Round(analyticsData.Average(a => a.ConversionRate), 2)
                : 0,
        };
    }

    public async Task<PostAnalyticsResponse> UpdateAsync(
        Guid postId, PostAnalyticsUpdateRequest request)
    {
        var post = await _context.Posts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == postId)
            ?? throw new NotFoundException("Post", postId);

        var analytics = await _context.PostAnalytics
            .FirstOrDefaultAsync(a => a.PostId == postId);

        if (analytics == null)
        {
            analytics = new PostAnalytics
            {
                PostId = postId,
                CreatedBy = _currentUser.Email,
            };
            _context.PostAnalytics.Add(analytics);
        }

        analytics.Views = request.Views;
        analytics.Clicks = request.Clicks;
        analytics.LeadsGenerated = request.LeadsGenerated;
        analytics.ConversionRate = request.Views > 0
            ? Math.Round((decimal)request.LeadsGenerated / request.Views * 100, 2)
            : 0;
        analytics.LastUpdated = DateTime.UtcNow;
        analytics.UpdatedAt = DateTime.UtcNow;
        analytics.UpdatedBy = _currentUser.Email;

        await _context.SaveChangesAsync();

        return MapToResponse(analytics, post.Title);
    }

    private static PostAnalyticsResponse MapToResponse(PostAnalytics analytics, string postTitle)
    {
        return new PostAnalyticsResponse
        {
            Id = analytics.Id,
            PostId = analytics.PostId,
            PostTitle = postTitle,
            Views = analytics.Views,
            Clicks = analytics.Clicks,
            LeadsGenerated = analytics.LeadsGenerated,
            ConversionRate = analytics.ConversionRate,
            LastUpdated = analytics.LastUpdated,
        };
    }
}
