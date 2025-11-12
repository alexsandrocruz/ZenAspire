using System.ComponentModel.DataAnnotations;
using CleanAspire.Domain.Common;
using CleanAspire.Domain.Enums;

namespace CleanAspire.Domain.Entities;

/// <summary>
/// Represents a customer segment with dynamic rules for membership.
/// Supports JSON-based rule definitions for flexible segmentation.
/// </summary>
public class Segment : BaseAuditableEntity, IAuditTrial
{
    /// <summary>
    /// Tenant identifier for multi-tenancy support
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Segment name (unique per tenant)
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional segment description
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// JSON definition of segment rules using the DSL
    /// Example: {"all": [{"field": "ClientType", "op": "eq", "value": "School"}]}
    /// </summary>
    [Required]
    public string DefinitionJson { get; set; } = string.Empty;

    /// <summary>
    /// Whether the segment is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When the segment was last rebuilt
    /// </summary>
    public DateTime? LastRebuiltAt { get; set; }

    /// <summary>
    /// Total number of members (cached)
    /// </summary>
    public int? MemberCount { get; set; }

    /// <summary>
    /// Navigation property for segment memberships
    /// </summary>
    public virtual ICollection<SegmentMembership> Memberships { get; set; } = new List<SegmentMembership>();

    /// <summary>
    /// Target owner types for this segment
    /// Multiple types supported (e.g., Client + Contact)
    /// </summary>
    public ICollection<OwnerType> TargetOwnerTypes { get; set; } = new List<OwnerType>();
}