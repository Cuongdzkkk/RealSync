using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealSync.Core.Entities;

namespace RealSync.Data.Configurations;

public class PropertyImageConfiguration : IEntityTypeConfiguration<PropertyImage>
{
    public void Configure(EntityTypeBuilder<PropertyImage> builder)
    {
        builder.ToTable("PropertyImages");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Url)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.Caption)
            .HasMaxLength(500);

        builder.HasOne(e => e.Property)
            .WithMany(p => p.Images)
            .HasForeignKey(e => e.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.PropertyId);
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
