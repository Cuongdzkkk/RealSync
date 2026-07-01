using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealSync.Core.Entities;

namespace RealSync.Data.Configurations;

public class VideoProjectConfiguration : IEntityTypeConfiguration<VideoProject>
{
    public void Configure(EntityTypeBuilder<VideoProject> builder)
    {
        builder.ToTable("VideoProjects");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.TargetChannel)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.AspectRatio)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.TargetDurationSeconds)
            .IsRequired();

        builder.Property(e => e.Status)
            .IsRequired();

        builder.Property(e => e.ApprovedStoryboardVersion)
            .IsRequired();

        // Relationships
        builder.HasOne(e => e.Post)
            .WithMany()
            .HasForeignKey(e => e.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.ContentVariant)
            .WithMany()
            .HasForeignKey(e => e.ContentVariantId)
            .OnDelete(DeleteBehavior.Restrict); // Avoid multiple cascade paths

        builder.HasOne(e => e.FinalAsset)
            .WithMany()
            .HasForeignKey(e => e.FinalAssetId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(e => new { e.Status, e.CreatedAt });

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
