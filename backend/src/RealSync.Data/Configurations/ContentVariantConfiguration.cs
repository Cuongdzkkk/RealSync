using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealSync.Core.Entities;

namespace RealSync.Data.Configurations;

public class ContentVariantConfiguration : IEntityTypeConfiguration<ContentVariant>
{
    public void Configure(EntityTypeBuilder<ContentVariant> builder)
    {
        builder.ToTable("ContentVariants");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ChannelType)
            .IsRequired();

        builder.Property(e => e.Language)
            .HasMaxLength(10)
            .HasDefaultValue("vi");

        builder.Property(e => e.Title)
            .HasMaxLength(500);

        builder.Property(e => e.Caption)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.Summary)
            .HasMaxLength(1000);

        builder.Property(e => e.HashtagsJson)
            .HasMaxLength(500);

        builder.Property(e => e.CallToAction)
            .HasMaxLength(200);

        builder.Property(e => e.LinkUrl)
            .HasMaxLength(1000);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Draft");

        builder.Property(e => e.ApprovedBy)
            .HasMaxLength(200);

        // Relationships
        builder.HasOne(e => e.Post)
            .WithMany() // Can navigate through ContentVariants if we added to Post, but for clean domain, Post doesn't necessarily need direct variants collection.
            .HasForeignKey(e => e.PostId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.SourceGeneration)
            .WithMany()
            .HasForeignKey(e => e.SourceGenerationId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(e => e.PostId);
        builder.HasIndex(e => new { e.PostId, e.ChannelType, e.Version });

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
