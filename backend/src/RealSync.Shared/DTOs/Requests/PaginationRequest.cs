namespace RealSync.Shared.DTOs.Requests;

/// <summary>
/// Request chuẩn cho phân trang, sắp xếp, tìm kiếm.
/// Dùng cho tất cả list/search endpoints.
/// </summary>
public class PaginationRequest
{
    private int _page = 1;
    private int _pageSize = 20;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value switch
        {
            < 1 => 20,
            > 100 => 100,
            _ => value
        };
    }

    public string? SortBy { get; set; }
    public string SortDirection { get; set; } = "desc";  // asc, desc
    public string? Search { get; set; }
}
