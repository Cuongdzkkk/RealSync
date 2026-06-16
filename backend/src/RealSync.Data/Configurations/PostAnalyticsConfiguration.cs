using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealSync.Core.Entities;

namespace RealSync.Data.Configurations;

/// <summary>
/// Configuration cho PostAnalytics entity.
/// Quan hệ 1-1 với Post.
/// </summary>
public class PostAnalyticsConfiguration : IEntityTypeConfiguration<PostAnalytics>
{
    public void Configure(EntityTypeBuilder<PostAnalytics> builder)
    {
        builder.ToTable("PostAnalytics");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ConversionRate)
            .HasColumnType("decimal(5,2)");

        // 1-1 relationship với Post
        builder.HasOne(e => e.Post)
            .WithOne(p => p.PostAnalytics)
            .HasForeignKey<PostAnalytics>(e => e.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => e.PostId).IsUnique();

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
