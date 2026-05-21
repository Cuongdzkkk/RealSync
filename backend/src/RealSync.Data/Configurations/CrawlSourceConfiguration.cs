using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealSync.Core.Entities;

namespace RealSync.Data.Configurations;

public class CrawlSourceConfiguration : IEntityTypeConfiguration<CrawlSource>
{
    public void Configure(EntityTypeBuilder<CrawlSource> builder)
    {
        builder.ToTable("CrawlSources");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.BaseUrl)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.CrawlConfig).HasColumnType("nvarchar(max)");
        builder.Property(e => e.CronSchedule).HasMaxLength(50);

        builder.HasIndex(e => e.Name).IsUnique();
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
