using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealSync.Core.Entities;

namespace RealSync.Data.Configurations;

public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.ToTable("Leads");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Phone).HasMaxLength(20);
        builder.Property(e => e.Email).HasMaxLength(200);

        builder.Property(e => e.Status)
            .HasMaxLength(50)
            .HasDefaultValue("New");

        builder.Property(e => e.Priority)
            .HasMaxLength(20)
            .HasDefaultValue("Normal");

        builder.Property(e => e.Budget)
            .HasColumnType("decimal(18,0)");

        builder.Property(e => e.Requirements)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.PreferredArea).HasMaxLength(200);
        builder.Property(e => e.PreferredType).HasMaxLength(100);
        builder.Property(e => e.SourceChannel).HasMaxLength(100);

        // Relationships
        builder.HasOne(e => e.InterestedProperty)
            .WithMany()
            .HasForeignKey(e => e.InterestedPropertyId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.AssignedTo)
            .WithMany(u => u.AssignedLeads)
            .HasForeignKey(e => e.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.AssignedToId);
        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => e.Score);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
