using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealSync.Core.Entities;

namespace RealSync.Data.Configurations;

public class OAuthCredentialConfiguration : IEntityTypeConfiguration<OAuthCredential>
{
    public void Configure(EntityTypeBuilder<OAuthCredential> builder)
    {
        builder.ToTable("OAuthCredentials");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.SecretReference)
            .HasMaxLength(500);

        builder.Property(e => e.EncryptionKeyVersion)
            .HasMaxLength(100);

        builder.Property(e => e.AccessTokenEncrypted)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.RefreshTokenEncrypted)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.CredentialStatus)
            .IsRequired();

        builder.Property(e => e.LastRefreshError)
            .HasMaxLength(2000);

        // Indexes
        builder.HasIndex(e => e.ConnectedAccountId);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
