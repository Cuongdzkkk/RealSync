using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealSync.Core.Entities;

namespace RealSync.Data.Configurations;

public class VideoSceneConfiguration : IEntityTypeConfiguration<VideoScene>
{
    public void Configure(EntityTypeBuilder<VideoScene> builder)
    {
        builder.ToTable("VideoScenes");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Sequence)
            .IsRequired();

        builder.Property(e => e.DurationSeconds)
            .IsRequired();

        builder.Property(e => e.Narration)
            .HasMaxLength(2000);

        builder.Property(e => e.OnScreenText)
            .HasMaxLength(1000);

        builder.Property(e => e.VisualPrompt)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.NegativePrompt)
            .HasMaxLength(1000);

        builder.Property(e => e.CameraDirection)
            .HasMaxLength(200);

        builder.Property(e => e.ReferenceAssetIdsJson)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        builder.Property(e => e.Status)
            .IsRequired();

        // Relationships
        builder.HasOne(e => e.VideoProject)
            .WithMany(p => p.Scenes)
            .HasForeignKey(e => e.VideoProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.GeneratedAsset)
            .WithMany()
            .HasForeignKey(e => e.GeneratedAssetId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(e => new { e.VideoProjectId, e.Sequence });

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
