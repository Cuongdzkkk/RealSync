using System;
using System.Collections.Generic;
using RealSync.Core.Enums;

namespace RealSync.Core.Entities;

/// <summary>
/// Biến thể nội dung của một Post cho một kênh đăng bài cụ thể.
/// </summary>
public class ContentVariant : BaseEntity
{
    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;

    public PublishingChannelType ChannelType { get; set; }
    public string? Language { get; set; } = "vi";
    public string? Title { get; set; }
    public string? Caption { get; set; }
    public string? Summary { get; set; }
    public string? HashtagsJson { get; set; }
    public string? CallToAction { get; set; }
    public string? LinkUrl { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, InReview, Approved, Rejected
    public Guid? SourceGenerationId { get; set; }
    public AIContentGeneration? SourceGeneration { get; set; }
    public int Version { get; set; } = 1;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }

    // Navigation
    public ICollection<PublicationJob> PublicationJobs { get; set; } = new List<PublicationJob>();
}
