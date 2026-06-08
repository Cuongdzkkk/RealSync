using Microsoft.EntityFrameworkCore;
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
    private readonly RealSyncDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public PostChannelService(RealSyncDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
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

        // Simulate publish action — cập nhật status
        channel.PublishStatus = "Published";
        channel.PublishedAt = DateTime.UtcNow;
        channel.UpdatedAt = DateTime.UtcNow;
        channel.UpdatedBy = _currentUser.Email;

        await _context.SaveChangesAsync();

        return MapToResponse(channel);
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
