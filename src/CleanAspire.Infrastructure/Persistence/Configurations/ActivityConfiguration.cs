using CleanAspire.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanAspire.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the Activity entity.
/// Phase 3: Timeline - Activities & Interactions
/// </summary>
public class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> builder)
    {
        // Table configuration
        builder.ToTable("Activities");

        // Primary key
        builder.HasKey(x => x.Id);

        // ✅ Multi-tenancy: TenantId required
        builder.Property(x => x.TenantId)
            .IsRequired()
            .HasMaxLength(450);

        // ✅ Index for tenant queries (performance)
        builder.HasIndex(x => x.TenantId);

        // ✅ Composite indexes for common queries
        builder.HasIndex(x => new { x.TenantId, x.Status, x.Due });
        builder.HasIndex(x => new { x.TenantId, x.AssignedToUserId, x.Status });
        builder.HasIndex(x => new { x.TenantId, x.RegardingType, x.RegardingId });

        // ✅ Index for reminder queries
        builder.HasIndex(x => new { x.ReminderAt, x.Status })
            .HasFilter($"\"{nameof(Activity.ReminderAt)}\" IS NOT NULL AND \"{nameof(Activity.Status)}\" = 1"); // Open status

        // Required properties
        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(ActivityStatus.Open);

        builder.Property(x => x.Priority)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(ActivityPriority.Normal);

        builder.Property(x => x.Start)
            .IsRequired();

        builder.Property(x => x.Subject)
            .IsRequired()
            .HasMaxLength(160);

        // Optional properties
        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.RegardingType)
            .HasConversion<int>();

        builder.Property(x => x.RegardingId);

        builder.Property(x => x.AssignedToUserId)
            .HasMaxLength(450);

        builder.Property(x => x.Due);

        builder.Property(x => x.ReminderAt);

        builder.Property(x => x.CompletedAt);

        builder.Property(x => x.Location)
            .HasMaxLength(200);

        builder.Property(x => x.DurationMinutes);

        // Indexes for filtering
        builder.HasIndex(x => x.Type);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Priority);
        builder.HasIndex(x => x.AssignedToUserId);
        builder.HasIndex(x => x.Start);
        builder.HasIndex(x => x.Due);

        // Ignore computed properties
        builder.Ignore(x => x.IsOverdue);
        builder.Ignore(x => x.IsCompleted);
        builder.Ignore(x => x.IsCanceled);

        // Ignore domain events
        builder.Ignore(e => e.DomainEvents);
    }
}
