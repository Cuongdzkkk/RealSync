namespace RealSync.Core.Entities;

/// <summary>
/// Khách hàng tiềm năng.
/// Schema theo database-guide.md Section 4.2.
/// </summary>
public class Lead : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }

    // Classification
    public string Status { get; set; } = "New";  // New, Contacted, Qualified, Proposal, Won, Lost
    public string Priority { get; set; } = "Normal";  // Low, Normal, High, Urgent
    public int Score { get; set; } = 0;  // Lead score (0-100)

    // Interest
    public Guid? InterestedPropertyId { get; set; }
    public Property? InterestedProperty { get; set; }
    public decimal? Budget { get; set; }
    public string? Requirements { get; set; }
    public string? PreferredArea { get; set; }
    public string? PreferredType { get; set; }

    // Assignment
    public Guid? AssignedToId { get; set; }
    public User? AssignedTo { get; set; }
    public string? SourceChannel { get; set; }  // Website, Facebook, Zalo, Phone, Referral

    // Dates
    public DateTime? LastContactedAt { get; set; }
    public DateTime? NextFollowUpAt { get; set; }
    public DateTime? ConvertedAt { get; set; }

    // Navigation
    public ICollection<LeadActivity> Activities { get; set; } = new List<LeadActivity>();
}
