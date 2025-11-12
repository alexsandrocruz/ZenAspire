// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanAspire.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Address entity
/// </summary>
public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Addresses");

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

        // Address fields
        builder.Property(x => x.Line1)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Line2)
            .HasMaxLength(200);

        builder.Property(x => x.District)
            .HasMaxLength(100);

        builder.Property(x => x.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.State)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Zip)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Country)
            .IsRequired()
            .HasMaxLength(2)
            .HasDefaultValue("BR");

        builder.Property(x => x.Label)
            .HasMaxLength(50);

        // Geocoding fields - use appropriate precision and scale
        builder.Property(x => x.GeoLat)
            .HasColumnType("decimal(10,7)"); // Latitude: -90 to +90, 7 decimal places

        builder.Property(x => x.GeoLng)
            .HasColumnType("decimal(10,7)"); // Longitude: -180 to +180, 7 decimal places

        builder.Property(x => x.IsPrimary)
            .IsRequired()
            .HasDefaultValue(false);

        // Indexes for performance
        // Index for tenant queries
        builder.HasIndex(x => x.TenantId);

        // Index for owner lookups (most common query pattern)
        builder.HasIndex(x => new { x.TenantId, x.OwnerType, x.OwnerId });

        // Index for finding primary addresses
        builder.HasIndex(x => new { x.TenantId, x.OwnerType, x.OwnerId, x.IsPrimary });

        // Index for geocoding queries
        builder.HasIndex(x => new { x.GeoLat, x.GeoLng })
            .HasFilter($"\"{nameof(Address.GeoLat)}\" IS NOT NULL AND \"{nameof(Address.GeoLng)}\" IS NOT NULL");

        // Ignore calculated properties
        builder.Ignore(x => x.FullAddress);

        // Ignore domain events
        builder.Ignore(e => e.DomainEvents);
    }
}
