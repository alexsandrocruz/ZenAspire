using CleanAspire.Domain.Enums;

namespace CleanAspire.Domain.Entities;

/// <summary>
/// Represents membership of an entity (Client/Contact) in a segment.
/// This is a materialized view for performance optimization.
/// </summary>
public class SegmentMembership
{
    /// <summary>
    /// Segment identifier
    /// </summary>
    public Guid SegmentId { get; set; }

    /// <summary>
    /// Tenant identifier for multi-tenancy support
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Type of entity that owns this membership
    /// </summary>
    public OwnerType OwnerType { get; set; }

    /// <summary>
    /// ID of the entity (Client/Contact/etc.)
    /// </summary>
    public Guid OwnerId { get; set; }

    /// <summary>
    /// When this membership was computed
    /// </summary>
    public DateTime ComputedAt { get; set; }

    /// <summary>
    /// Navigation property to the segment
    /// </summary>
    public virtual Segment Segment { get; set; } = null!;
}

/// <summary>
/// Composite key configuration for SegmentMembership
/// Used by EF Core to define the composite primary key
/// </summary>
public class SegmentMembershipConfiguration
{
    /// <summary>
    /// Defines the composite key for SegmentMembership entity
    /// Key: (SegmentId, TenantId, OwnerType, OwnerId)
    /// </summary>
    public static object[] CompositeKey => new object[] { nameof(SegmentMembership.SegmentId), nameof(SegmentMembership.TenantId), nameof(SegmentMembership.OwnerType), nameof(SegmentMembership.OwnerId) };
}