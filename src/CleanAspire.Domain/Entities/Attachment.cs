// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel.DataAnnotations;
using CleanAspire.Domain.Common;

namespace CleanAspire.Domain.Entities;

/// <summary>
/// Represents a file attachment linked to a Note
/// Files are stored in blob storage (MinIO/Azure Blob)
/// </summary>
public class Attachment : BaseAuditableEntity, IAuditTrial
{
    /// <summary>
    /// Tenant identifier for multi-tenancy isolation
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to Note
    /// </summary>
    [Required]
    public string NoteId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property to Note
    /// </summary>
    public virtual Note Note { get; set; } = null!;

    /// <summary>
    /// Original filename (e.g., "proposal.pdf")
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Content/MIME type (e.g., "application/pdf", "image/jpeg")
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Blob storage key/path for retrieving the file
    /// Format: {tenantId}/{noteId}/{attachmentId}/{filename}
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string BlobKey { get; set; } = string.Empty;

    /// <summary>
    /// Optional file hash (SHA256) for integrity verification
    /// </summary>
    [MaxLength(64)]
    public string? FileHash { get; set; }
}
