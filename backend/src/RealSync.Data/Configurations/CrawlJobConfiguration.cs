using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealSync.Core.Entities;

namespace RealSync.Data.Configurations;

public class CrawlJobConfiguration : IEntityTypeConfiguration<CrawlJob>
{
    public void Configure(EntityTypeBuilder<CrawlJob> builder)
    {
        builder.ToTable("CrawlJobs");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status)
            .HasMaxLength(50)
            .HasDefaultValue("Pending");

        builder.Property(e => e.ErrorMessage)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.ExecutionLog)
            .HasColumnType("nvarchar(max)");

        builder.HasOne(e => e.CrawlSource)
            .WithMany(cs => cs.CrawlJobs)
            .HasForeignKey(e => e.CrawlSourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.CrawlSourceId);
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
