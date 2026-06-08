using Microsoft.EntityFrameworkCore;
using RealSync.Core.Entities;
using RealSync.Core.Interfaces;
using RealSync.Data.Context;
using RealSync.Shared.DTOs.Requests.Posts;
using RealSync.Shared.DTOs.Responses.Posts;
using RealSync.Shared.Exceptions;

namespace RealSync.Services.Implementations;

/// <summary>
/// Service lên lịch đăng bài.
/// Note: Hangfire background job sẽ được tích hợp ở giai đoạn sau.
/// Hiện tại chỉ quản lý schedule records.
/// </summary>
public class PostScheduleService : IPostScheduleService
{
    private readonly RealSyncDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public PostScheduleService(RealSyncDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<PostScheduleResponse>> GetByPostIdAsync(Guid postId)
    {
        var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
        if (!postExists)
            throw new NotFoundException("Post", postId);

        var schedules = await _context.PostSchedules
            .Include(s => s.Post)
            .Where(s => s.PostId == postId)
            .AsNoTracking()
            .OrderBy(s => s.ScheduledAt)
            .ToListAsync();

        return schedules.Select(MapToResponse);
    }

    public async Task<IEnumerable<PostScheduleResponse>> GetUpcomingAsync(int count = 20)
    {
        var now = DateTime.UtcNow;

        var schedules = await _context.PostSchedules
            .Include(s => s.Post)
            .Where(s => s.Status == "Pending" && s.ScheduledAt > now)
            .AsNoTracking()
            .OrderBy(s => s.ScheduledAt)
            .Take(count)
            .ToListAsync();

        return schedules.Select(MapToResponse);
    }

    public async Task<PostScheduleResponse> CreateAsync(Guid postId, ScheduleCreateRequest request)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == postId)
            ?? throw new NotFoundException("Post", postId);

        if (request.ScheduledAt <= DateTime.UtcNow)
            throw new BusinessException("Thời gian lên lịch phải ở tương lai.");

        var schedule = new PostSchedule
        {
            PostId = postId,
            ScheduledAt = request.ScheduledAt,
            Status = "Pending",
            CreatedBy = _currentUser.Email,
        };

        _context.PostSchedules.Add(schedule);

        // Cập nhật post status thành Scheduled
        if (post.Status == "Draft")
        {
            post.Status = "Scheduled";
            post.UpdatedAt = DateTime.UtcNow;
            post.UpdatedBy = _currentUser.Email;
        }

        await _context.SaveChangesAsync();

        schedule.Post = post;
        return MapToResponse(schedule);
    }

    public async Task DeleteAsync(Guid postId, Guid id)
    {
        var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
        if (!postExists)
            throw new NotFoundException("Post", postId);

        var schedule = await _context.PostSchedules
            .FirstOrDefaultAsync(s => s.Id == id && s.PostId == postId)
            ?? throw new NotFoundException("PostSchedule", id);

        if (schedule.Status != "Pending")
            throw new BusinessException("Chỉ có thể hủy lịch ở trạng thái Pending.");

        schedule.Status = "Cancelled";
        schedule.UpdatedAt = DateTime.UtcNow;
        schedule.UpdatedBy = _currentUser.Email;

        await _context.SaveChangesAsync();
    }

    private static PostScheduleResponse MapToResponse(PostSchedule schedule)
    {
        return new PostScheduleResponse
        {
            Id = schedule.Id,
            PostId = schedule.PostId,
            PostTitle = schedule.Post?.Title ?? string.Empty,
            ScheduledAt = schedule.ScheduledAt,
            Status = schedule.Status,
            CreatedAt = schedule.CreatedAt,
        };
    }
}
