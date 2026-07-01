using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealSync.Core.Entities;

namespace RealSync.Data.Configurations;

public class PublicationJobConfiguration : IEntityTypeConfiguration<PublicationJob>
{
    public void Configure(EntityTypeBuilder<PublicationJob> builder)
    {
        builder.ToTable("PublicationJobs");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.PublishMode)
            .IsRequired();

        builder.Property(e => e.Status)
            .IsRequired();

        builder.Property(e => e.IdempotencyKey)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(e => e.PayloadSnapshotJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.MediaManifestJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.ExternalPostId)
            .HasMaxLength(200);

        builder.Property(e => e.ExternalPublishId)
            .HasMaxLength(200);

        builder.Property(e => e.PublishedUrl)
            .HasMaxLength(1000);

        builder.Property(e => e.RemoteStatus)
            .HasMaxLength(100);

        builder.Property(e => e.LastErrorCategory)
            .HasMaxLength(100);

        builder.Property(e => e.LastErrorCode)
            .HasMaxLength(100);

        builder.Property(e => e.LastErrorMessage)
            .HasMaxLength(2000);

        builder.Property(e => e.CorrelationId)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(e => e.Post)
            .WithMany()
            .HasForeignKey(e => e.PostId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ContentVariant)
            .WithMany(cv => cv.PublicationJobs)
            .HasForeignKey(e => e.ContentVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ConnectedAccount)
            .WithMany(ca => ca.PublicationJobs)
            .HasForeignKey(e => e.ConnectedAccountId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(e => new { e.Status, e.ScheduledAtUtc });
        builder.HasIndex(e => e.IdempotencyKey).IsUnique();
        builder.HasIndex(e => new { e.ConnectedAccountId, e.CreatedAt });

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
