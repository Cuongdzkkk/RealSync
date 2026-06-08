namespace RealSync.Shared.DTOs.Requests.Posts;

/// <summary>
/// Request đổi trạng thái bài đăng (PATCH).
/// </summary>
public class PostStatusUpdateRequest
{
    public string Status { get; set; } = string.Empty;
}
