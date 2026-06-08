using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealSync.Core.Entities;

namespace RealSync.Data.Configurations;

/// <summary>
/// Configuration cho PostSchedule entity.
/// </summary>
public class PostScheduleConfiguration : IEntityTypeConfiguration<PostSchedule>
{
    public void Configure(EntityTypeBuilder<PostSchedule> builder)
    {
        builder.ToTable("PostSchedules");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status)
            .HasMaxLength(50)
            .HasDefaultValue("Pending");

        // Relationships
        builder.HasOne(e => e.Post)
            .WithMany(p => p.PostSchedules)
            .HasForeignKey(e => e.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => e.PostId);
        builder.HasIndex(e => e.ScheduledAt);
        builder.HasIndex(e => e.Status);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
