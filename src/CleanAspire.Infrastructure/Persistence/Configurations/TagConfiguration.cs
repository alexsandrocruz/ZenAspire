// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanAspire.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the Tag entity.
/// </summary>
public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("Tags");

        builder.HasKey(x => x.Id);

        // Multi-tenancy: TenantId required
        builder.Property(x => x.TenantId)
            .IsRequired()
            .HasMaxLength(450);

        // Tag name required, unique per tenant
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Color)
            .HasMaxLength(7);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        // Composite unique index per tenant: (TenantId, Name)
        builder.HasIndex(x => new { x.TenantId, x.Name })
            .IsUnique();

        // Index for tenant queries (performance)
        builder.HasIndex(x => x.TenantId);

        // Relationship: Tag 1:N TagLinks
        builder.HasMany(x => x.TagLinks)
            .WithOne(x => x.Tag)
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
