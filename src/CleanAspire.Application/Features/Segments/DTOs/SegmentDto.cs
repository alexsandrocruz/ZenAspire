using CleanAspire.Domain.Enums;

namespace CleanAspire.Application.Features.Segments.DTOs;

/// <summary>
/// Data Transfer Object for Segment entity.
/// Used for API responses and client-server communication.
/// </summary>
public class SegmentDto
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Segment name (unique per tenant)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional segment description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// JSON definition of segment rules
    /// </summary>
    public string DefinitionJson { get; set; } = string.Empty;

    /// <summary>
    /// Whether the segment is currently active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// When the segment was last rebuilt
    /// </summary>
    public DateTime? LastRebuiltAt { get; set; }

    /// <summary>
    /// Total number of members (cached)
    /// </summary>
    public int? MemberCount { get; set; }

    /// <summary>
    /// Target owner types for this segment
    /// </summary>
    public List<OwnerType> TargetOwnerTypes { get; set; } = new();

    /// <summary>
    /// Formatted target owner types for display
    /// </summary>
    public string TargetOwnerTypesDisplay => string.Join(", ", TargetOwnerTypes);

    /// <summary>
    /// Whether the segment needs rebuilding (based on LastRebuiltAt)
    /// </summary>
    public bool NeedsRebuild => !LastRebuiltAt.HasValue ||
                               (DateTime.UtcNow - LastRebuiltAt.Value).TotalHours > 24;

    // Audit Information
    public DateTime? Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }

    // Status indicators
    public string StatusDisplay
    {
        get
        {
            if (!IsActive) return "Inactive";
            if (!LastRebuiltAt.HasValue) return "Never Built";
            if (NeedsRebuild) return "Stale";
            return "Active";
        }
    }

    /// <summary>
    /// Formatted last rebuilt date
    /// </summary>
    public string LastRebuiltDisplay => LastRebuiltAt?.ToString("yyyy-MM-dd HH:mm") ?? "Never";
}