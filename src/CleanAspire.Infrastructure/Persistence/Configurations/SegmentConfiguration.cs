using CleanAspire.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanAspire.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the Segment entity.
/// </summary>
public class SegmentConfiguration : IEntityTypeConfiguration<Segment>
{
    public void Configure(EntityTypeBuilder<Segment> builder)
    {
        // Table configuration
        builder.ToTable("Segments");

        // Primary key
        builder.HasKey(x => x.Id);

        // Multi-tenancy: TenantId required
        builder.Property(x => x.TenantId)
            .IsRequired()
            .HasMaxLength(450);

        // Composite unique index per tenant: (TenantId, Name)
        builder.HasIndex(x => new { x.TenantId, x.Name })
            .IsUnique()
            .HasDatabaseName("IX_Segments_TenantId_Name_Unique");

        // Index for tenant queries (performance)
        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("IX_Segments_TenantId");

        // Index for active segments
        builder.HasIndex(x => new { x.TenantId, x.IsActive })
            .HasDatabaseName("IX_Segments_TenantId_IsActive");

        // Index for last rebuilt tracking
        builder.HasIndex(x => x.LastRebuiltAt)
            .HasDatabaseName("IX_Segments_LastRebuiltAt");

        // Required properties
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.DefinitionJson)
            .IsRequired();

        // Optional properties
        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.MemberCount);

        // JSON column for PostgreSQL jsonb support
        builder.Property(x => x.DefinitionJson)
            .HasColumnType("jsonb");

        // Configure relationship with memberships
        builder.HasMany(x => x.Memberships)
            .WithOne(x => x.Segment)
            .HasForeignKey(x => x.SegmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(e => e.DomainEvents);
    }
}