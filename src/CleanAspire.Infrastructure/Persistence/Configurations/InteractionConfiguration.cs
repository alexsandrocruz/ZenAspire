using CleanAspire.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanAspire.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the Interaction entity.
/// Phase 3: Timeline - Activities & Interactions
/// </summary>
public class InteractionConfiguration : IEntityTypeConfiguration<Interaction>
{
    public void Configure(EntityTypeBuilder<Interaction> builder)
    {
        // Table configuration
        builder.ToTable("Interactions");

        // Primary key
        builder.HasKey(x => x.Id);

        // ✅ Multi-tenancy: TenantId required
        builder.Property(x => x.TenantId)
            .IsRequired()
            .HasMaxLength(450);

        // ✅ Index for tenant queries (performance)
        builder.HasIndex(x => x.TenantId);

        // ✅ Composite indexes for common timeline queries
        builder.HasIndex(x => new { x.TenantId, x.OwnerType, x.OwnerId, x.At })
            .HasDatabaseName("IX_Interactions_Timeline"); // Timeline query optimization

        builder.HasIndex(x => new { x.TenantId, x.Type, x.At });
        builder.HasIndex(x => new { x.TenantId, x.Direction, x.At });
        builder.HasIndex(x => new { x.TenantId, x.HandledByUserId, x.At });

        // ✅ Unique constraint for deduplication (prevent duplicate imports)
        builder.HasIndex(x => new { x.TenantId, x.ChannelRef, x.Type })
            .IsUnique()
            .HasFilter($"\"{nameof(Interaction.ChannelRef)}\" IS NOT NULL")
            .HasDatabaseName("IX_Interactions_ChannelRef_Unique");

        // Required properties
        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Direction)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue(InteractionDirection.Outbound);

        builder.Property(x => x.At)
            .IsRequired();

        builder.Property(x => x.OwnerType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.OwnerId)
            .IsRequired();

        // Optional properties
        builder.Property(x => x.ChannelRef)
            .HasMaxLength(256);

        builder.Property(x => x.Subject)
            .HasMaxLength(200);

        builder.Property(x => x.Snippet)
            .HasMaxLength(500);

        // ✅ JSON payload for channel-specific data
        builder.Property(x => x.PayloadJson)
            .HasColumnType("jsonb"); // PostgreSQL jsonb, for SQLite will use text

        builder.Property(x => x.DurationSeconds);

        builder.Property(x => x.HandledByUserId)
            .HasMaxLength(450);

        builder.Property(x => x.Sentiment)
            .HasMaxLength(20);

        builder.Property(x => x.Tags)
            .HasMaxLength(500);

        // Indexes for filtering and analytics
        builder.HasIndex(x => x.Type);
        builder.HasIndex(x => x.Direction);
        builder.HasIndex(x => x.At);
        builder.HasIndex(x => x.OwnerType);
        builder.HasIndex(x => x.HandledByUserId);
        builder.HasIndex(x => x.Sentiment);

        // ✅ Full-text search on Subject and Snippet (PostgreSQL)
        // Uncomment for PostgreSQL:
        // builder.HasIndex(x => new { x.Subject, x.Snippet })
        //     .HasMethod("GIN")
        //     .IsTsVectorExpressionIndex("english");

        // Ignore computed properties
        builder.Ignore(x => x.IsInbound);
        builder.Ignore(x => x.IsOutbound);

        // Ignore domain events
        builder.Ignore(e => e.DomainEvents);
    }
}
