// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanAspire.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the Note entity.
/// </summary>
public class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.ToTable("Notes");

        builder.HasKey(x => x.Id);

        // Multi-tenancy: TenantId required
        builder.Property(x => x.TenantId)
            .IsRequired()
            .HasMaxLength(450);

        // Polymorphic owner
        builder.Property(x => x.OwnerType)
            .IsRequired()
            .HasConversion<int>(); // Store enum as int

        builder.Property(x => x.OwnerId)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(200);

        builder.Property(x => x.Body)
            .IsRequired();

        builder.Property(x => x.IsPinned)
            .HasDefaultValue(false);

        builder.Property(x => x.IsPrivate)
            .HasDefaultValue(false);

        // Index for tenant queries
        builder.HasIndex(x => x.TenantId);

        // Index for finding all notes for an owner
        builder.HasIndex(x => new { x.TenantId, x.OwnerType, x.OwnerId });

        // Index for pinned notes
        builder.HasIndex(x => new { x.TenantId, x.OwnerType, x.OwnerId, x.IsPinned });

        // Relationship: Note 1:N Attachments
        builder.HasMany(x => x.Attachments)
            .WithOne(x => x.Note)
            .HasForeignKey(x => x.NoteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
