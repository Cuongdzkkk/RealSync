using RealSync.Shared.DTOs.Requests.Posts;
using RealSync.Shared.DTOs.Responses.Posts;

namespace RealSync.Core.Interfaces;

/// <summary>
/// Interface cho Post service — CRUD bài đăng.
/// </summary>
public interface IPostService
{
    Task<PostResponse> GetByIdAsync(Guid id);
    Task<(IEnumerable<PostResponse> Items, int TotalCount)> GetListAsync(PostFilterRequest filter);
    Task<PostResponse> CreateAsync(PostCreateRequest request);
    Task<PostResponse> UpdateAsync(Guid id, PostUpdateRequest request);
    Task<PostResponse> UpdateStatusAsync(Guid id, string status);
    Task DeleteAsync(Guid id);
}
