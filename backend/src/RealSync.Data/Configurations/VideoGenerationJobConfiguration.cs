using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealSync.Core.Entities;

namespace RealSync.Data.Configurations;

public class VideoGenerationJobConfiguration : IEntityTypeConfiguration<VideoGenerationJob>
{
    public void Configure(EntityTypeBuilder<VideoGenerationJob> builder)
    {
        builder.ToTable("VideoGenerationJobs");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Provider)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Model)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.OperationId)
            .HasMaxLength(200);

        builder.Property(e => e.Status)
            .IsRequired();

        builder.Property(e => e.ErrorCategory)
            .HasMaxLength(100);

        builder.Property(e => e.ErrorCode)
            .HasMaxLength(100);

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(2000);

        // Relationships
        builder.HasOne(e => e.VideoProject)
            .WithMany()
            .HasForeignKey(e => e.VideoProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.VideoScene)
            .WithMany()
            .HasForeignKey(e => e.VideoSceneId)
            .OnDelete(DeleteBehavior.Restrict); // Set to Restrict to avoid multiple cascade paths in SQL Server

        builder.HasOne(e => e.OutputAsset)
            .WithMany()
            .HasForeignKey(e => e.OutputAssetId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(e => new { e.Status, e.OperationId });

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
