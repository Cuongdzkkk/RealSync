using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealSync.Core.Entities;

namespace RealSync.Data.Configurations;

public class LeadActivityConfiguration : IEntityTypeConfiguration<LeadActivity>
{
    public void Configure(EntityTypeBuilder<LeadActivity> builder)
    {
        builder.ToTable("LeadActivities");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ActivityType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Description)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.OldValue).HasMaxLength(500);
        builder.Property(e => e.NewValue).HasMaxLength(500);

        builder.HasOne(e => e.Lead)
            .WithMany(l => l.Activities)
            .HasForeignKey(e => e.LeadId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.PerformedBy)
            .WithMany()
            .HasForeignKey(e => e.PerformedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.LeadId);
        builder.HasIndex(e => e.CreatedAt);
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
