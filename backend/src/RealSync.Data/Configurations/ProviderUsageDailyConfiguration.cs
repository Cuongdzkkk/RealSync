using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealSync.Core.Entities;

namespace RealSync.Data.Configurations;

public class ProviderUsageDailyConfiguration : IEntityTypeConfiguration<ProviderUsageDaily>
{
    public void Configure(EntityTypeBuilder<ProviderUsageDaily> builder)
    {
        builder.ToTable("ProviderUsageDaily");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.UsageDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(e => e.EstimatedCost)
            .HasPrecision(18, 4);

        // Relationships
        builder.HasOne(e => e.ProviderCredential)
            .WithMany(pc => pc.ProviderUsageDailies)
            .HasForeignKey(e => e.ProviderCredentialId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => new { e.ProviderCredentialId, e.UsageDate });

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
