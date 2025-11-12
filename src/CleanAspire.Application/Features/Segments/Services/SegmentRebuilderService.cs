using CleanAspire.Application.Common.Interfaces;
using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Models;
using CleanAspire.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CleanAspire.Application.Features.Segments.Services;

/// <summary>
/// Service for rebuilding segments using the rule engine
/// </summary>
public class SegmentRebuilderService : ISegmentRebuilderService
{
    private readonly IApplicationDbContext _context;
    private readonly ISegmentRuleEngine _ruleEngine;
    private readonly ILogger<SegmentRebuilderService> _logger;
    private readonly ICurrentUserService _currentUser;

    public SegmentRebuilderService(
        IApplicationDbContext context,
        ISegmentRuleEngine ruleEngine,
        ILogger<SegmentRebuilderService> logger,
        ICurrentUserService currentUser)
    {
        _context = context;
        _ruleEngine = ruleEngine;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<SegmentRebuildResult> RebuildSegmentAsync(
        Guid segmentId,
        bool forceRebuild = false,
        CancellationToken cancellationToken = default)
    {
        var result = new SegmentRebuildResult
        {
            SegmentId = segmentId,
            StartTime = DateTime.UtcNow
        };

        try
        {
            _logger.LogInformation("Starting rebuild for segment {SegmentId} (Force: {ForceRebuild})", segmentId, forceRebuild);

            // Validate tenant
            if (string.IsNullOrEmpty(_currentUser.TenantId))
            {
                result.ErrorMessage = "TenantId is required";
                return result;
            }

            // Get segment
            var segment = await _context.Segments
                .FirstOrDefaultAsync(s => s.Id == segmentId.ToString() && s.TenantId == _currentUser.TenantId, cancellationToken);

            if (segment == null)
            {
                result.ErrorMessage = $"Segment with ID {segmentId} not found";
                return result;
            }

            if (!segment.IsActive)
            {
                result.ErrorMessage = "Cannot rebuild inactive segment";
                return result;
            }

            // Check if rebuild is needed (unless forced)
            if (!forceRebuild && segment.LastRebuiltAt.HasValue)
            {
                var timeSinceRebuild = DateTime.UtcNow - segment.LastRebuiltAt.Value;
                if (timeSinceRebuild.TotalMinutes < 30)
                {
                    result.Success = true;
                    result.ErrorMessage = "Segment was rebuilt recently (skip)";
                    result.NewMemberCount = segment.MemberCount ?? 0;
                    return result;
                }
            }

            result.SegmentName = segment.Name;
            result.PreviousMemberCount = segment.MemberCount ?? 0;

            // Parse and validate segment definition
            var validationResult = await _ruleEngine.ValidateSegmentDefinitionAsync(segment.DefinitionJson, cancellationToken);
            if (!validationResult.IsValid)
            {
                result.ErrorMessage = $"Invalid segment definition: {string.Join(", ", validationResult.Errors)}";
                return result;
            }

            // Parse segment definition
            var segmentDefinition = await _ruleEngine.ParseSegmentDefinitionAsync(segment.DefinitionJson, cancellationToken);
            if (segmentDefinition == null)
            {
                result.ErrorMessage = "Failed to parse segment definition";
                return result;
            }

            // Evaluate segment rules
            var evaluationResult = await _ruleEngine.EvaluateSegmentAsync(segmentDefinition, cancellationToken);
            if (!evaluationResult.Success)
            {
                result.ErrorMessage = $"Rule evaluation failed: {string.Join(", ", evaluationResult.Errors)}";
                return result;
            }

            // Get current memberships for comparison
            var existingMemberships = await _context.SegmentMemberships
                .Where(sm => sm.SegmentId == segmentId.ToString() && sm.TenantId == _currentUser.TenantId)
                .ToListAsync(cancellationToken);

            // Clear existing memberships
            _context.SegmentMemberships.RemoveRange(existingMemberships);
            await _context.SaveChangesAsync(cancellationToken);

            // Create new memberships
            var newMemberships = evaluationResult.Matches.Select(match => new SegmentMembership
            {
                SegmentId = segmentId.ToString(),
                TenantId = _currentUser.TenantId,
                OwnerType = match.EntityType == "Client" ? OwnerType.Client : OwnerType.Contact,
                OwnerId = match.EntityId,
                ComputedAt = DateTime.UtcNow
            }).ToList();

            _context.SegmentMemberships.AddRange(newMemberships);
            await _context.SaveChangesAsync(cancellationToken);

            // Update segment metadata
            segment.LastRebuiltAt = DateTime.UtcNow;
            segment.MemberCount = newMemberships.Count;
            _context.Segments.Update(segment);
            await _context.SaveChangesAsync(cancellationToken);

            // Calculate results
            result.Success = true;
            result.NewMemberCount = newMemberships.Count;
            result.MembersAdded = Math.Max(0, newMemberships.Count - result.PreviousMemberCount);
            result.MembersRemoved = Math.Max(0, result.PreviousMemberCount - newMemberships.Count);
            result.TotalEntitiesEvaluated = evaluationResult.TotalEntitiesEvaluated;

            // Add warnings from evaluation
            result.Warnings.AddRange(evaluationResult.Errors);

            _logger.LogInformation(
                "Successfully rebuilt segment {SegmentName}: {NewCount} members (added: {Added}, removed: {Removed}) in {Duration}ms",
                segment.Name, result.NewMemberCount, result.MembersAdded, result.MembersRemoved,
                result.Duration.TotalMilliseconds);

            // TODO: Send notification about rebuild completion
            // This could be implemented later with notification services
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rebuilding segment {SegmentId}", segmentId);
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }
        finally
        {
            result.EndTime = DateTime.UtcNow;
            result.Duration = result.EndTime - result.StartTime;
        }

        return result;
    }

    public async Task<List<SegmentRebuildResult>> RebuildAllSegmentsAsync(
        CancellationToken cancellationToken = default)
    {
        var results = new List<SegmentRebuildResult>();

        try
        {
            _logger.LogInformation("Starting rebuild for all active segments");

            // Validate tenant
            if (string.IsNullOrEmpty(_currentUser.TenantId))
            {
                _logger.LogError("TenantId is required for rebuilding all segments");
                return results;
            }

            // Get all active segments
            var activeSegments = await _context.Segments
                .Where(s => s.TenantId == _currentUser.TenantId && s.IsActive)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Found {Count} active segments to rebuild", activeSegments.Count);

            // Rebuild each segment
            foreach (var segment in activeSegments)
            {
                var segmentId = Guid.Parse(segment.Id);
                var result = await RebuildSegmentAsync(segmentId, forceRebuild: true, cancellationToken);
                results.Add(result);

                // Add delay between rebuilds to prevent overwhelming the system
                await Task.Delay(100, cancellationToken);
            }

            var successCount = results.Count(r => r.Success);
            var failureCount = results.Count(r => !r.Success);

            _logger.LogInformation(
                "Rebuild all segments completed: {SuccessCount} successful, {FailureCount} failed",
                successCount, failureCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rebuilding all segments");
        }

        return results;
    }

    public async Task<SegmentRebuildStats> GetRebuildStatsAsync(
        Guid segmentId,
        CancellationToken cancellationToken = default)
    {
        var stats = new SegmentRebuildStats { SegmentId = segmentId };

        try
        {
            // Validate tenant
            if (string.IsNullOrEmpty(_currentUser.TenantId))
            {
                return stats;
            }

            // Get segment with member counts
            var segmentData = await _context.Segments
                .Where(s => s.Id == segmentId.ToString() && s.TenantId == _currentUser.TenantId)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.LastRebuiltAt,
                    s.MemberCount
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (segmentData == null)
            {
                return stats;
            }

            stats.SegmentName = segmentData.Name;
            stats.TotalMembers = segmentData.MemberCount ?? 0;
            stats.LastRebuiltAt = segmentData.LastRebuiltAt;

            // Get member counts by type
            var memberCounts = await _context.SegmentMemberships
                .Where(sm => sm.SegmentId == segmentId.ToString() && sm.TenantId == _currentUser.TenantId)
                .GroupBy(sm => sm.OwnerType)
                .Select(g => new { OwnerType = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            stats.ClientMembers = memberCounts.FirstOrDefault(c => c.OwnerType == OwnerType.Client)?.Count ?? 0;
            stats.ContactMembers = memberCounts.FirstOrDefault(c => c.OwnerType == OwnerType.Contact)?.Count ?? 0;

            // Calculate if rebuild is needed
            if (stats.LastRebuiltAt.HasValue)
            {
                var timeSinceRebuild = DateTime.UtcNow - stats.LastRebuiltAt.Value;
                stats.NeedsRebuild = timeSinceRebuild.TotalHours > 24;

                // Calculate rebuild frequency (rebuilds per day)
                stats.RebuildFrequency = 1.0 / Math.Max(1, timeSinceRebuild.TotalDays);
            }
            else
            {
                stats.NeedsRebuild = true;
            }

            // TODO: Calculate last rebuild duration from logs or tracking table
            // For now, we'll estimate based on member count
            stats.LastRebuildDuration = TimeSpan.FromMilliseconds(Math.Max(500, stats.TotalMembers * 10));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting rebuild stats for segment {SegmentId}", segmentId);
        }

        return stats;
    }
}