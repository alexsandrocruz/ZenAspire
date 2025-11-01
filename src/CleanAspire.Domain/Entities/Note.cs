// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel.DataAnnotations;
using CleanAspire.Domain.Common;
using CleanAspire.Domain.Enums;

namespace CleanAspire.Domain.Entities;

/// <summary>
/// Represents a note/comment that can be attached to various entities (Client, Contact, Activity, etc.)
/// Supports rich text content and file attachments.
/// </summary>
public class Note : BaseAuditableEntity, IAuditTrial
{
    /// <summary>
    /// Tenant identifier for multi-tenancy isolation
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Type of entity this note belongs to (Client, Contact, Activity, etc.)
    /// </summary>
    [Required]
    public OwnerType OwnerType { get; set; }

    /// <summary>
    /// ID of the entity this note belongs to (polymorphic)
    /// </summary>
    [Required]
    public string OwnerId { get; set; } = string.Empty;

    /// <summary>
    /// Optional title for the note
    /// </summary>
    [MaxLength(200)]
    public string? Title { get; set; }

    /// <summary>
    /// Note content/body (supports plain text or markdown)
    /// </summary>
    [Required]
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Whether this note is pinned (shows at top)
    /// </summary>
    public bool IsPinned { get; set; }

    /// <summary>
    /// Whether this note is private (only visible to creator)
    /// </summary>
    public bool IsPrivate { get; set; }

    /// <summary>
    /// Navigation property to attachments
    /// </summary>
    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
}
