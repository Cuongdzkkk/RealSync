using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealSync.Core.Entities;

namespace RealSync.Data.Configurations;

public class FollowUpReminderDispatchConfiguration : IEntityTypeConfiguration<FollowUpReminderDispatch>
{
    public void Configure(EntityTypeBuilder<FollowUpReminderDispatch> builder)
    {
        builder.ToTable("FollowUpReminderDispatches");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ScheduledFor)
            .IsRequired();

        builder.Property(x => x.SentAt)
            .IsRequired();

        builder.HasOne(x => x.Lead)
            .WithMany()
            .HasForeignKey(x => x.LeadId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Notification)
            .WithMany()
            .HasForeignKey(x => x.NotificationId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new { x.LeadId, x.ScheduledFor })
            .IsUnique();

        builder.HasIndex(x => x.ScheduledFor);
        builder.HasIndex(x => x.SentAt);
    }
}
