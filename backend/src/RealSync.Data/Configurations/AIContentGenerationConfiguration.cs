using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealSync.Core.Entities;

namespace RealSync.Data.Configurations;

/// <summary>
/// Configuration cho AIContentGeneration entity.
/// </summary>
public class AIContentGenerationConfiguration : IEntityTypeConfiguration<AIContentGeneration>
{
    public void Configure(EntityTypeBuilder<AIContentGeneration> builder)
    {
        builder.ToTable("AIContentGenerations");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Prompt)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.GeneratedContent)
            .HasColumnType("nvarchar(max)");

        // Relationships
        builder.HasOne(e => e.Post)
            .WithMany(p => p.AIContentGenerations)
            .HasForeignKey(e => e.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => e.PostId);
        builder.HasIndex(e => e.CreatedAt);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
