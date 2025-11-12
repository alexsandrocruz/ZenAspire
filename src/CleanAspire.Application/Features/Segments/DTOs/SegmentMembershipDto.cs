using CleanAspire.Domain.Enums;

namespace CleanAspire.Application.Features.Segments.DTOs;

/// <summary>
/// Data Transfer Object for SegmentMembership entity.
/// Used for API responses and client-server communication.
/// </summary>
public class SegmentMembershipDto
{
    public Guid SegmentId { get; set; }
    public string SegmentName { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public OwnerType OwnerType { get; set; }
    public string OwnerTypeDisplay { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public DateTime ComputedAt { get; set; }

    /// <summary>
    /// Formatted computed date
    /// </summary>
    public string ComputedAtDisplay => ComputedAt.ToString("yyyy-MM-dd HH:mm");
}

/// <summary>
/// DTO for segment statistics and metrics
/// </summary>
public class SegmentStatsDto
{
    public Guid SegmentId { get; set; }
    public string SegmentName { get; set; } = string.Empty;
    public int TotalMembers { get; set; }
    public int ClientMembers { get; set; }
    public int ContactMembers { get; set; }
    public DateTime? LastRebuiltAt { get; set; }
    public TimeSpan? RebuildDuration { get; set; }
    public int RulesCount { get; set; }
    public bool IsActive { get; set; }

    /// <summary>
    /// Percentage breakdown by owner type
    /// </summary>
    public decimal ClientPercentage => TotalMembers > 0 ? (decimal)ClientMembers / TotalMembers * 100 : 0;
    public decimal ContactPercentage => TotalMembers > 0 ? (decimal)ContactMembers / TotalMembers * 100 : 0;
}

/// <summary>
/// DTO for creating or updating a segment
/// </summary>
public class CreateUpdateSegmentDto
{
    /// <summary>
    /// Segment name (unique per tenant)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional segment description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Target owner types for this segment
    /// </summary>
    public List<OwnerType> TargetOwnerTypes { get; set; } = new();

    /// <summary>
    /// JSON definition of segment rules
    /// </summary>
    public string DefinitionJson { get; set; } = string.Empty;

    /// <summary>
    /// Whether the segment should be active
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO for segment rebuild operation
/// </summary>
public class RebuildSegmentDto
{
    public Guid SegmentId { get; set; }
    public bool ForceRebuild { get; set; } = false;
    public string? TriggeredBy { get; set; }
}

/// <summary>
/// DTO for segment member filtering and pagination
/// </summary>
public class SegmentMembersFilterDto
{
    public Guid SegmentId { get; set; }
    public OwnerType? OwnerType { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
}