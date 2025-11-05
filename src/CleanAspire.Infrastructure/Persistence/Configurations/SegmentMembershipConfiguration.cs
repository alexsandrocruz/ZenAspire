using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanAspire.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the SegmentMembership entity.
/// Uses composite key for optimal performance in segment membership queries.
/// </summary>
public class SegmentMembershipConfiguration : IEntityTypeConfiguration<SegmentMembership>
{
    public void Configure(EntityTypeBuilder<SegmentMembership> builder)
    {
        // Table configuration
        builder.ToTable("SegmentMemberships");

        // Composite primary key: (SegmentId, TenantId, OwnerType, OwnerId)
        builder.HasKey(x => new { x.SegmentId, x.TenantId, x.OwnerType, x.OwnerId })
            .HasName("PK_SegmentMemberships");

        // Required properties
        builder.Property(x => x.TenantId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.OwnerType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.ComputedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes for performance
        // 1. Query segment members: (SegmentId, TenantId, OwnerType)
        builder.HasIndex(x => new { x.SegmentId, x.TenantId, x.OwnerType })
            .HasDatabaseName("IX_SegmentMemberships_Segment_Tenant_Type");

        // 2. Query entity segments: (TenantId, OwnerType, OwnerId)
        builder.HasIndex(x => new { x.TenantId, x.OwnerType, x.OwnerId })
            .HasDatabaseName("IX_SegmentMemberships_Tenant_Type_Owner");

        // 3. Query by computed timestamp: (ComputedAt)
        builder.HasIndex(x => x.ComputedAt)
            .HasDatabaseName("IX_SegmentMemberships_ComputedAt");

        // 4. Index for rebuild operations: (SegmentId, ComputedAt)
        builder.HasIndex(x => new { x.SegmentId, x.ComputedAt })
            .HasDatabaseName("IX_SegmentMemberships_Segment_ComputedAt");

        // Configure relationship with Segment
        builder.HasOne(x => x.Segment)
            .WithMany(x => x.Memberships)
            .HasForeignKey(x => x.SegmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // No auto-incrementing key since we use composite key
        builder.Property(x => x.SegmentId)
            .ValueGeneratedNever();

        builder.Property(x => x.OwnerId)
            .ValueGeneratedNever();
    }
}