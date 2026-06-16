using Microsoft.EntityFrameworkCore;
using RealSync.Core.Entities;
using RealSync.Core.Interfaces;
using RealSync.Data.Context;
using RealSync.Shared.DTOs.Requests.Posts;
using RealSync.Shared.DTOs.Responses.Posts;
using RealSync.Shared.Exceptions;

namespace RealSync.Services.Implementations;

/// <summary>
/// Service xử lý CRUD bài đăng BĐS.
/// </summary>
public class PostService : IPostService
{
    private readonly RealSyncDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public PostService(RealSyncDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PostResponse> GetByIdAsync(Guid id)
    {
        var post = await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Property)
            .Include(p => p.PostChannels)
            .Include(p => p.PostSchedules)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException("Post", id);

        return MapToResponse(post);
    }

    public async Task<(IEnumerable<PostResponse> Items, int TotalCount)> GetListAsync(
        PostFilterRequest filter)
    {
        var query = _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Property)
            .Include(p => p.PostChannels)
            .Include(p => p.PostSchedules)
            .AsNoTracking()
            .AsQueryable();

        // Filters
        if (!string.IsNullOrWhiteSpace(filter.Status))
            query = query.Where(p => p.Status == filter.Status);

        if (filter.AuthorId.HasValue)
            query = query.Where(p => p.AuthorId == filter.AuthorId.Value);

        if (filter.PropertyId.HasValue)
            query = query.Where(p => p.PropertyId == filter.PropertyId.Value);

        // Search
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(p =>
                p.Title.ToLower().Contains(search) ||
                (p.Summary != null && p.Summary.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync();

        // Sort
        query = filter.SortBy?.ToLower() switch
        {
            "title" => filter.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(p => p.Title)
                : query.OrderByDescending(p => p.Title),
            "status" => filter.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(p => p.Status)
                : query.OrderByDescending(p => p.Status),
            "publishedat" => filter.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(p => p.PublishedAt)
                : query.OrderByDescending(p => p.PublishedAt),
            _ => filter.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(p => p.CreatedAt)
                : query.OrderByDescending(p => p.CreatedAt),
        };

        // Pagination
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return (items.Select(MapToResponse), totalCount);
    }

    public async Task<PostResponse> CreateAsync(PostCreateRequest request)
    {
        // Validate PropertyId nếu có
        if (request.PropertyId.HasValue)
        {
            var propertyExists = await _context.Properties
                .AnyAsync(p => p.Id == request.PropertyId.Value);
            if (!propertyExists)
                throw new NotFoundException("Property", request.PropertyId.Value);
        }

        var post = new Post
        {
            Title = request.Title,
            Content = request.Content,
            Summary = request.Summary,
            ThumbnailUrl = request.ThumbnailUrl,
            PropertyId = request.PropertyId,
            AuthorId = _currentUser.UserId
                ?? throw new UnauthorizedException("Không xác định được người dùng."),
            Status = "Draft",
            CreatedBy = _currentUser.Email,
        };

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(post.Id);
    }

    public async Task<PostResponse> UpdateAsync(Guid id, PostUpdateRequest request)
    {
        var post = await _context.Posts.FindAsync(id)
            ?? throw new NotFoundException("Post", id);

        // Validate PropertyId nếu có
        if (request.PropertyId.HasValue)
        {
            var propertyExists = await _context.Properties
                .AnyAsync(p => p.Id == request.PropertyId.Value);
            if (!propertyExists)
                throw new NotFoundException("Property", request.PropertyId.Value);
        }

        post.Title = request.Title;
        post.Content = request.Content;
        post.Summary = request.Summary;
        post.ThumbnailUrl = request.ThumbnailUrl;
        post.PropertyId = request.PropertyId;
        post.UpdatedAt = DateTime.UtcNow;
        post.UpdatedBy = _currentUser.Email;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(post.Id);
    }

    public async Task<PostResponse> UpdateStatusAsync(Guid id, string status)
    {
        var post = await _context.Posts.FindAsync(id)
            ?? throw new NotFoundException("Post", id);

        post.Status = status;
        post.UpdatedAt = DateTime.UtcNow;
        post.UpdatedBy = _currentUser.Email;

        if (status == "Published" && post.PublishedAt == null)
            post.PublishedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(post.Id);
    }

    public async Task DeleteAsync(Guid id)
    {
        var post = await _context.Posts.FindAsync(id)
            ?? throw new NotFoundException("Post", id);

        post.IsDeleted = true;
        post.DeletedAt = DateTime.UtcNow;
        post.UpdatedBy = _currentUser.Email;

        await _context.SaveChangesAsync();
    }

    private static PostResponse MapToResponse(Post post)
    {
        return new PostResponse
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            Summary = post.Summary,
            ThumbnailUrl = post.ThumbnailUrl,
            Status = post.Status,
            PublishedAt = post.PublishedAt,
            AuthorId = post.AuthorId,
            AuthorName = post.Author?.FullName ?? string.Empty,
            PropertyId = post.PropertyId,
            PropertyTitle = post.Property?.Title ?? null,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt,
            ChannelCount = post.PostChannels?.Count ?? 0,
            ScheduleCount = post.PostSchedules?.Count ?? 0,
        };
    }
}
