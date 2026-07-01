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

        builder.Property(e => e.PromptTokens)
            .IsRequired(false);

        builder.Property(e => e.CompletionTokens)
            .IsRequired(false);

        builder.Property(e => e.EstimatedCost)
            .HasPrecision(18, 9)
            .IsRequired(false);

        builder.Property(e => e.FactsUsedJson)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

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
