// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel.DataAnnotations;
using CleanAspire.Domain.Common;
using CleanAspire.Domain.Enums;

namespace CleanAspire.Domain.Entities;

/// <summary>
/// Polymorphic M:N link between Tags and various entities (Client, Contact, Activity, etc.)
/// Enables tagging any entity type with normalized tags.
/// </summary>
public class TagLink : BaseEntity
{
    /// <summary>
    /// Tenant identifier for multi-tenancy isolation
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to Tag
    /// </summary>
    [Required]
    public string TagId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property to Tag
    /// </summary>
    public virtual Tag Tag { get; set; } = null!;

    /// <summary>
    /// Type of entity being tagged (Client, Contact, Activity, etc.)
    /// </summary>
    [Required]
    public OwnerType OwnerType { get; set; }

    /// <summary>
    /// ID of the entity being tagged (polymorphic)
    /// </summary>
    [Required]
    public string OwnerId { get; set; } = string.Empty;

    /// <summary>
    /// When this tag was linked to the entity
    /// </summary>
    public DateTime LinkedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User who linked this tag
    /// </summary>
    [MaxLength(450)]
    public string? LinkedBy { get; set; }
}
