using RealSync.Shared.DTOs.Requests;

namespace RealSync.Shared.DTOs.Requests.Posts;

/// <summary>
/// Request filter/search bài đăng.
/// Kế thừa PaginationRequest để có sẵn Page, PageSize, SortBy, SortDirection, Search.
/// </summary>
public class PostFilterRequest : PaginationRequest
{
    public string? Status { get; set; }
    public Guid? AuthorId { get; set; }
    public Guid? PropertyId { get; set; }
}
