using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealSync.Core.Entities;

namespace RealSync.Data.Configurations;

public class ConnectedAccountConfiguration : IEntityTypeConfiguration<ConnectedAccount>
{
    public void Configure(EntityTypeBuilder<ConnectedAccount> builder)
    {
        builder.ToTable("ConnectedAccounts");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Provider)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.ChannelType)
            .IsRequired();

        builder.Property(e => e.DisplayName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.ExternalAccountId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.ExternalParentAccountId)
            .HasMaxLength(200);

        builder.Property(e => e.ProfileUrl)
            .HasMaxLength(1000);

        builder.Property(e => e.AvatarUrl)
            .HasMaxLength(1000);

        builder.Property(e => e.Status)
            .IsRequired();

        builder.Property(e => e.GrantedScopesJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.LastErrorCode)
            .HasMaxLength(100);

        builder.Property(e => e.LastErrorMessage)
            .HasMaxLength(2000);

        // Relationships
        builder.HasOne(e => e.OAuthCredential)
            .WithOne(o => o.ConnectedAccount)
            .HasForeignKey<OAuthCredential>(o => o.ConnectedAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => new { e.WorkspaceId, e.Provider, e.Status });
        builder.HasIndex(e => e.ExternalAccountId);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
