using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealSync.Core.Entities;

namespace RealSync.Data.Configurations;

/// <summary>
/// Configuration cho Post entity.
/// </summary>
public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("Posts");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Content)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.Summary)
            .HasMaxLength(1000);

        builder.Property(e => e.ThumbnailUrl)
            .HasMaxLength(1000);

        builder.Property(e => e.Status)
            .HasMaxLength(50)
            .HasDefaultValue("Draft");

        // Relationships
        builder.HasOne(e => e.Property)
            .WithMany(p => p.Posts)
            .HasForeignKey(e => e.PropertyId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Author)
            .WithMany(u => u.Posts)
            .HasForeignKey(e => e.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.AuthorId);
        builder.HasIndex(e => e.PropertyId);
        builder.HasIndex(e => e.PublishedAt);
        builder.HasIndex(e => e.CreatedAt);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
