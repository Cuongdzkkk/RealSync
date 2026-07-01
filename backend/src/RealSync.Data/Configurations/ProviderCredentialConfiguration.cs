using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealSync.Core.Entities;

namespace RealSync.Data.Configurations;

public class ProviderCredentialConfiguration : IEntityTypeConfiguration<ProviderCredential>
{
    public void Configure(EntityTypeBuilder<ProviderCredential> builder)
    {
        builder.ToTable("ProviderCredentials");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Provider)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.CredentialType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.SecretReference)
            .HasMaxLength(500);

        builder.Property(e => e.Status)
            .IsRequired();

        builder.Property(e => e.AllowedCapabilitiesJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.BudgetDaily)
            .HasPrecision(18, 4);

        builder.Property(e => e.BudgetMonthly)
            .HasPrecision(18, 4);

        builder.Property(e => e.QuotaMetadataJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.DisabledReason)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(e => new { e.WorkspaceId, e.Provider, e.Status });

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
