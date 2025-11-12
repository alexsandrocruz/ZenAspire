// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanAspire.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the Attachment entity.
/// </summary>
public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable("Attachments");

        builder.HasKey(x => x.Id);

        // Multi-tenancy: TenantId required
        builder.Property(x => x.TenantId)
            .IsRequired()
            .HasMaxLength(450);

        // Foreign key to Note
        builder.Property(x => x.NoteId)
            .IsRequired();

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Size)
            .IsRequired();

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.BlobKey)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.FileHash)
            .HasMaxLength(64);

        // Index for tenant queries
        builder.HasIndex(x => x.TenantId);

        // Index for finding all attachments for a note
        builder.HasIndex(x => x.NoteId);

        // Index for blob key lookups
        builder.HasIndex(x => x.BlobKey)
            .IsUnique();
    }
}
