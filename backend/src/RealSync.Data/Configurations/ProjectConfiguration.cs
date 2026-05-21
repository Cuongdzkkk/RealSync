using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealSync.Core.Entities;

namespace RealSync.Data.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(e => e.Description)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.Slug)
            .HasMaxLength(300);

        builder.Property(e => e.DeveloperName)
            .HasMaxLength(200);

        builder.Property(e => e.Address)
            .HasMaxLength(500);

        builder.Property(e => e.Status)
            .HasMaxLength(50);

        builder.Property(e => e.TotalArea)
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.ImageUrl)
            .HasMaxLength(1000);

        builder.HasOne(e => e.Area)
            .WithMany()
            .HasForeignKey(e => e.AreaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.Slug).IsUnique()
            .HasFilter("[Slug] IS NOT NULL");
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
