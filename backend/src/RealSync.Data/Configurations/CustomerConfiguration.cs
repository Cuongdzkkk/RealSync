using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealSync.Core.Entities;

namespace RealSync.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Phone)
            .HasMaxLength(20);

        builder.Property(c => c.Email)
            .HasMaxLength(200);

        builder.Property(c => c.Address)
            .HasMaxLength(500);

        builder.Property(c => c.Company)
            .HasMaxLength(200);

        builder.Property(c => c.Notes)
            .HasMaxLength(2000);

        builder.Property(c => c.Source)
            .HasMaxLength(50);

        // Relationships
        builder.HasOne(c => c.ConvertedFromLead)
            .WithMany()
            .HasForeignKey(c => c.ConvertedFromLeadId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.AssignedTo)
            .WithMany(u => u.AssignedCustomers)
            .HasForeignKey(c => c.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(c => c.Email);
        builder.HasIndex(c => c.Phone);
        builder.HasIndex(c => c.AssignedToId);
    }
}
