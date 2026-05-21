using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealSync.Core.Entities;

namespace RealSync.Data.Configurations;

public class CrawlResultConfiguration : IEntityTypeConfiguration<CrawlResult>
{
    public void Configure(EntityTypeBuilder<CrawlResult> builder)
    {
        builder.ToTable("CrawlResults");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Url)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.Status)
            .HasMaxLength(50)
            .HasDefaultValue("Pending");

        builder.Property(e => e.RawData)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.ErrorMessage)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.ContentHash)
            .HasMaxLength(64);

        builder.HasOne(e => e.CrawlJob)
            .WithMany(cj => cj.Results)
            .HasForeignKey(e => e.CrawlJobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Property)
            .WithMany()
            .HasForeignKey(e => e.PropertyId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.CrawlJobId);
        builder.HasIndex(e => e.ContentHash);
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
