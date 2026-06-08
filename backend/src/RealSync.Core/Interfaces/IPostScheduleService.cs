using RealSync.Shared.DTOs.Requests.Posts;
using RealSync.Shared.DTOs.Responses.Posts;

namespace RealSync.Core.Interfaces;

/// <summary>
/// Interface cho Post Schedule service — lên lịch đăng bài.
/// </summary>
public interface IPostScheduleService
{
    Task<IEnumerable<PostScheduleResponse>> GetByPostIdAsync(Guid postId);
    Task<IEnumerable<PostScheduleResponse>> GetUpcomingAsync(int count = 20);
    Task<PostScheduleResponse> CreateAsync(Guid postId, ScheduleCreateRequest request);
    Task DeleteAsync(Guid postId, Guid id);
}
