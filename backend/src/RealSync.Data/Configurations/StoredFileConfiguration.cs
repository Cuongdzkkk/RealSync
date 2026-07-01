using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealSync.Core.Entities;

namespace RealSync.Data.Configurations;

public class StoredFileConfiguration : IEntityTypeConfiguration<StoredFile>
{
    public void Configure(EntityTypeBuilder<StoredFile> builder)
    {
        builder.ToTable("StoredFiles");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.OriginalFileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.StoredFileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.RelativePath)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Extension)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.Sha256)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(e => e.EntityType)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(e => new { e.EntityType, e.EntityId });
        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => e.Sha256);
        builder.HasIndex(e => e.IsDeleted);
        builder.HasIndex(e => e.RelativePath).IsUnique();

        // Query filter: exclude soft-deleted
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
