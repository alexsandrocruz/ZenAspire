// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanAspire.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the TagLink entity.
/// </summary>
public class TagLinkConfiguration : IEntityTypeConfiguration<TagLink>
{
    public void Configure(EntityTypeBuilder<TagLink> builder)
    {
        builder.ToTable("TagLinks");

        builder.HasKey(x => x.Id);

        // Multi-tenancy: TenantId required
        builder.Property(x => x.TenantId)
            .IsRequired()
            .HasMaxLength(450);

        // Foreign key to Tag
        builder.Property(x => x.TagId)
            .IsRequired();

        // Polymorphic owner
        builder.Property(x => x.OwnerType)
            .IsRequired()
            .HasConversion<int>(); // Store enum as int

        builder.Property(x => x.OwnerId)
            .IsRequired();

        builder.Property(x => x.LinkedAt)
            .IsRequired();

        builder.Property(x => x.LinkedBy)
            .HasMaxLength(450);

        // Composite unique index: one tag per owner (prevent duplicates)
        builder.HasIndex(x => new { x.TenantId, x.TagId, x.OwnerType, x.OwnerId })
            .IsUnique();

        // Index for tenant queries
        builder.HasIndex(x => x.TenantId);

        // Index for finding all tags for an owner
        builder.HasIndex(x => new { x.OwnerType, x.OwnerId });

        // Index for finding all owners of a tag
        builder.HasIndex(x => x.TagId);
    }
}
