using RealSync.Shared.DTOs.Requests.Posts;
using RealSync.Shared.DTOs.Responses.Posts;

namespace RealSync.Core.Interfaces;

/// <summary>
/// Interface cho Post Channel service — quản lý kênh đăng bài.
/// </summary>
public interface IPostChannelService
{
    Task<IEnumerable<PostChannelResponse>> GetByPostIdAsync(Guid postId);
    Task<PostChannelResponse> CreateAsync(Guid postId, PostChannelCreateRequest request);
    Task<PostChannelResponse> UpdateAsync(Guid postId, Guid id, PostChannelUpdateRequest request);
    Task DeleteAsync(Guid postId, Guid id);
    Task<PostChannelResponse> PublishAsync(Guid postId, Guid id);
}
