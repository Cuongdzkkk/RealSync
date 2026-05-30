using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealSync.Core.Entities;

namespace RealSync.Data.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Group)
            .HasMaxLength(50);

        builder.Property(p => p.Description)
            .HasMaxLength(300);

        // Unique constraint: tên permission phải duy nhất
        builder.HasIndex(p => p.Name).IsUnique();

        // Index cho group để query nhanh
        builder.HasIndex(p => p.Group);
    }
}
