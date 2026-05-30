namespace RealSync.Core.Entities;

/// <summary>
/// Người dùng hệ thống.
/// </summary>
public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    // Role
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    // Navigation
    public ICollection<Lead> AssignedLeads { get; set; } = new List<Lead>();
    public ICollection<Customer> AssignedCustomers { get; set; } = new List<Customer>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
}
