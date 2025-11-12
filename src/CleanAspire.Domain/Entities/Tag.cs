// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel.DataAnnotations;
using CleanAspire.Domain.Common;

namespace CleanAspire.Domain.Entities;

/// <summary>
/// Represents a tag that can be applied to various entities (Client, Contact, Activity, etc.)
/// Tags are tenant-scoped and normalized (no inline strings).
/// </summary>
public class Tag : BaseAuditableEntity, IAuditTrial
{
    /// <summary>
    /// Tenant identifier for multi-tenancy isolation
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Tag name (e.g., "VIP", "Hot Lead", "Priority")
    /// Unique per tenant
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional color for UI display (hex format: #FF5733)
    /// </summary>
    [MaxLength(7)]
    public string? Color { get; set; }

    /// <summary>
    /// Optional description
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Navigation property to TagLinks (entities tagged with this tag)
    /// </summary>
    public virtual ICollection<TagLink> TagLinks { get; set; } = new List<TagLink>();
}
