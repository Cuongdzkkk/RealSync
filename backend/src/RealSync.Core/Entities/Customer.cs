namespace RealSync.Core.Entities;

/// <summary>
/// Khách hàng đã chuyển đổi từ Lead (Won status).
/// Lưu thông tin chi tiết khách hàng chính thức.
/// </summary>
public class Customer : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Company { get; set; }
    public string? Notes { get; set; }
    public string? Source { get; set; }  // Website, Facebook, Zalo, Phone, Referral

    // Liên kết với Lead đã chuyển đổi
    public Guid? ConvertedFromLeadId { get; set; }
    public Lead? ConvertedFromLead { get; set; }

    // Người phụ trách
    public Guid? AssignedToId { get; set; }
    public User? AssignedTo { get; set; }
}
