using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealSync.Core.Entities;

namespace RealSync.Data.Configurations;

public class PropertyCategoryConfiguration : IEntityTypeConfiguration<PropertyCategory>
{
    public void Configure(EntityTypeBuilder<PropertyCategory> builder)
    {
        builder.ToTable("PropertyCategories");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Slug)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.HasIndex(e => e.Name).IsUnique()
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(e => e.Slug).IsUnique()
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(e => e.IsActive);
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
