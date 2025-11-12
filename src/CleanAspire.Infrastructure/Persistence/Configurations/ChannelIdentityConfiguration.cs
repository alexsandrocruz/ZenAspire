// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanAspire.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for ChannelIdentity entity
/// </summary>
public class ChannelIdentityConfiguration : IEntityTypeConfiguration<ChannelIdentity>
{
    public void Configure(EntityTypeBuilder<ChannelIdentity> builder)
    {
        builder.ToTable("ChannelIdentities");

        builder.HasKey(x => x.Id);

        // Tenant isolation
        builder.Property(x => x.TenantId)
            .IsRequired()
            .HasMaxLength(450);

        // Polymorphic owner relationship
        builder.Property(x => x.OwnerType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.OwnerId)
            .IsRequired()
            .HasMaxLength(450);

        // Channel fields
        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Value)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Label)
            .HasMaxLength(50);

        builder.Property(x => x.IsPrimary)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.OptedIn)
            .IsRequired()
            .HasDefaultValue(true);

        // Indexes for performance
        // Index for tenant queries
        builder.HasIndex(x => x.TenantId);

        // Index for owner lookups (most common query pattern)
        builder.HasIndex(x => new { x.TenantId, x.OwnerType, x.OwnerId });

        // Index for finding primary channels
        builder.HasIndex(x => new { x.TenantId, x.OwnerType, x.OwnerId, x.Type, x.IsPrimary });

        // Index for channel value lookups (e.g., finding by email/phone)
        builder.HasIndex(x => new { x.TenantId, x.Type, x.Value });

        // Unique constraint: prevent duplicate channel values per owner
        // (e.g., same email can't be added twice to the same client)
        builder.HasIndex(x => new { x.TenantId, x.OwnerType, x.OwnerId, x.Type, x.Value })
            .IsUnique();

        // Index for finding verified channels
        builder.HasIndex(x => new { x.TenantId, x.VerifiedAt })
            .HasFilter($"\"{nameof(ChannelIdentity.VerifiedAt)}\" IS NOT NULL");

        // Ignore calculated properties
        builder.Ignore(x => x.DisplayValue);
        builder.Ignore(x => x.IsVerified);

        // Ignore domain events
        builder.Ignore(e => e.DomainEvents);
    }
}
