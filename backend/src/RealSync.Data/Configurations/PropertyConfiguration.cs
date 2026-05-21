using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealSync.Core.Entities;

namespace RealSync.Data.Configurations;

/// <summary>
/// Configuration cho Property entity.
/// Theo mẫu database-guide.md Section 7.
/// </summary>
public class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.ToTable("Properties");
        builder.HasKey(e => e.Id);

        // Basic Info
        builder.Property(e => e.PropertyCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Description)
            .HasColumnType("nvarchar(max)");

        // Location
        builder.Property(e => e.Address).HasMaxLength(500);
        builder.Property(e => e.Ward).HasMaxLength(100);
        builder.Property(e => e.District).HasMaxLength(100);
        builder.Property(e => e.Province).HasMaxLength(100);
        builder.Property(e => e.Latitude).HasColumnType("decimal(10,8)");
        builder.Property(e => e.Longitude).HasColumnType("decimal(11,8)");

        // Specs — map Area_ property to "Area" column
        builder.Property(e => e.Area_)
            .HasColumnName("Area")
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.Price)
            .HasColumnType("decimal(18,0)");

        builder.Property(e => e.PriceUnit)
            .HasMaxLength(20)
            .HasDefaultValue("VND");

        builder.Property(e => e.Direction).HasMaxLength(20);
        builder.Property(e => e.LegalStatus).HasMaxLength(100);

        // Status
        builder.Property(e => e.Status)
            .HasMaxLength(50)
            .HasDefaultValue("Draft");

        builder.Property(e => e.ListingType)
            .HasMaxLength(20)
            .HasDefaultValue("Sale");

        // Source
        builder.Property(e => e.SourceType).HasMaxLength(50);
        builder.Property(e => e.SourceUrl).HasMaxLength(1000);

        // SEO
        builder.Property(e => e.Slug).HasMaxLength(500);
        builder.Property(e => e.MetaTitle).HasMaxLength(200);
        builder.Property(e => e.MetaDescription).HasMaxLength(500);

        // Relationships
        builder.HasOne(e => e.PropertyType)
            .WithMany(pt => pt.Properties)
            .HasForeignKey(e => e.PropertyTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Project)
            .WithMany(p => p.Properties)
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Area)
            .WithMany(a => a.Properties)
            .HasForeignKey(e => e.AreaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CrawlJob)
            .WithMany(cj => cj.Properties)
            .HasForeignKey(e => e.CrawlJobId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(e => e.PropertyCode).IsUnique();
        builder.HasIndex(e => e.Slug).IsUnique()
            .HasFilter("[Slug] IS NOT NULL");
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.PropertyTypeId);
        builder.HasIndex(e => e.AreaId);
        builder.HasIndex(e => e.Price);
        builder.HasIndex(e => e.ListingType);
        builder.HasIndex(e => e.CreatedAt);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
