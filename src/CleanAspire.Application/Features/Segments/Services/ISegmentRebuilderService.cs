using CleanAspire.Domain.Entities;

namespace CleanAspire.Application.Features.Segments.Services;

/// <summary>
/// Service interface for rebuilding segments
/// </summary>
public interface ISegmentRebuilderService
{
    /// <summary>
    /// Rebuilds a specific segment
    /// </summary>
    Task<SegmentRebuildResult> RebuildSegmentAsync(
        Guid segmentId,
        bool forceRebuild = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rebuilds all active segments
    /// </summary>
    Task<List<SegmentRebuildResult>> RebuildAllSegmentsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets rebuild statistics
    /// </summary>
    Task<SegmentRebuildStats> GetRebuildStatsAsync(
        Guid segmentId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of segment rebuild operation
/// </summary>
public class SegmentRebuildResult
{
    public Guid SegmentId { get; set; }
    public string SegmentName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int PreviousMemberCount { get; set; }
    public int NewMemberCount { get; set; }
    public int MembersAdded { get; set; }
    public int MembersRemoved { get; set; }
    public int TotalEntitiesEvaluated { get; set; }
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Rebuild statistics for a segment
/// </summary>
public class SegmentRebuildStats
{
    public Guid SegmentId { get; set; }
    public string SegmentName { get; set; } = string.Empty;
    public int TotalMembers { get; set; }
    public int ClientMembers { get; set; }
    public int ContactMembers { get; set; }
    public DateTime? LastRebuiltAt { get; set; }
    public TimeSpan? LastRebuildDuration { get; set; }
    public bool NeedsRebuild { get; set; }
    public double RebuildFrequency { get; set; }
}