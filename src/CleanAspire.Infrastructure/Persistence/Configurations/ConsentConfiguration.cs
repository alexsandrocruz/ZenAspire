using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanAspire.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the Consent entity.
/// Phase 4: LGPD Compliance - Consent & Data Privacy
/// </summary>
public class ConsentConfiguration : IEntityTypeConfiguration<Consent>
{
    public void Configure(EntityTypeBuilder<Consent> builder)
    {
        // Table configuration
        builder.ToTable("Consents");

        // Primary key
        builder.HasKey(x => x.Id);

        // Multi-tenancy: TenantId required
        builder.Property(x => x.TenantId)
            .IsRequired()
            .HasMaxLength(450);

        // Composite unique index per tenant: (TenantId, OwnerType, OwnerId, Purpose)
        // Ensures one consent record per owner per purpose per tenant
        builder.HasIndex(x => new { x.TenantId, x.OwnerType, x.OwnerId, x.Purpose })
            .IsUnique();

        // Index for tenant queries (performance)
        builder.HasIndex(x => x.TenantId);

        // Index for owner lookup queries
        builder.HasIndex(x => new { x.TenantId, x.OwnerType, x.OwnerId });
        builder.HasIndex(x => new { x.TenantId, x.Purpose });

        // Required properties
        builder.Property(x => x.OwnerType)
            .HasConversion<int>();

        builder.Property(x => x.Purpose)
            .HasConversion<int>();

        builder.Property(x => x.OptIn)
            .IsRequired();

        builder.Property(x => x.Channel)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.At)
            .IsRequired();

        // Optional properties
        builder.Property(x => x.Source)
            .HasMaxLength(200);

        builder.Property(x => x.IpAddress)
            .HasMaxLength(45);

        builder.Property(x => x.UserAgent)
            .HasMaxLength(500);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        builder.Property(x => x.ValidUntil);

        // Relationships - simplified without constraints due to polymorphic relationship
        // The OwnerType/OwnerId combination allows linking to different entity types
        // These relationships are handled at the application level, not database level

        // Additional indexes
        builder.HasIndex(x => x.At);
        builder.HasIndex(x => new { x.TenantId, x.OptIn });
        builder.HasIndex(x => new { x.TenantId, x.ValidUntil });

        // Ignore calculated properties
        builder.Ignore(x => x.IsActive);

        // Ignore domain events
        builder.Ignore(e => e.DomainEvents);
    }
}