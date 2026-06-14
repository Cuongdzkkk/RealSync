using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealSync.Core.Entities;

namespace RealSync.Data.Configurations;

/// <summary>
/// Configuration cho PostChannel entity.
/// </summary>
public class PostChannelConfiguration : IEntityTypeConfiguration<PostChannel>
{
    public void Configure(EntityTypeBuilder<PostChannel> builder)
    {
        builder.ToTable("PostChannels");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Channel)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.PublishStatus)
            .HasMaxLength(50)
            .HasDefaultValue("Pending");

        builder.Property(e => e.PublishedUrl)
            .HasMaxLength(1000);

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(2000);

        // Relationships
        builder.HasOne(e => e.Post)
            .WithMany(p => p.PostChannels)
            .HasForeignKey(e => e.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => e.PostId);
        builder.HasIndex(e => e.Channel);
        builder.HasIndex(e => e.PublishStatus);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
