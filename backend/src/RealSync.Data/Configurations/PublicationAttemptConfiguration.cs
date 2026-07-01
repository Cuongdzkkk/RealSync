using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealSync.Core.Entities;

namespace RealSync.Data.Configurations;

public class PublicationAttemptConfiguration : IEntityTypeConfiguration<PublicationAttempt>
{
    public void Configure(EntityTypeBuilder<PublicationAttempt> builder)
    {
        builder.ToTable("PublicationAttempts");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ProviderErrorCode)
            .HasMaxLength(100);

        builder.Property(e => e.NormalizedErrorCategory)
            .HasMaxLength(100);

        builder.Property(e => e.ProviderRequestId)
            .HasMaxLength(200);

        builder.Property(e => e.RequestMetadataJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.ResponseMetadataJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.RetryDecision)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(e => e.PublicationJob)
            .WithMany(j => j.PublicationAttempts)
            .HasForeignKey(e => e.PublicationJobId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => new { e.PublicationJobId, e.AttemptNumber });

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
