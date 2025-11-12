using CleanAspire.Application.Features.Segments.DTOs;
using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Enums;
using CleanAspire.Application.Pipeline;
using Microsoft.EntityFrameworkCore;

namespace CleanAspire.Application.Features.Segments.Queries;

/// <summary>
/// Query to fetch segment statistics and metrics.
/// </summary>
public record GetSegmentStatsQuery(Guid SegmentId) : IRequest<SegmentStatsDto>;

/// <summary>
/// Handler for the GetSegmentStatsQuery.
/// Retrieves segment statistics and metrics.
/// </summary>
public class GetSegmentStatsQueryHandler : IRequestHandler<GetSegmentStatsQuery, SegmentStatsDto>
{
    private readonly IApplicationDbContext _context;

    public GetSegmentStatsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<SegmentStatsDto> Handle(GetSegmentStatsQuery request, CancellationToken cancellationToken)
    {
        // Get segment with member counts
        var segmentStats = await _context.Segments
            .Where(s => s.Id == request.SegmentId.ToString())
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.IsActive,
                s.LastRebuiltAt,
                // Count total members
                TotalMembers = s.Memberships.Count,
                // Count members by type
                ClientMembers = s.Memberships.Count(m => m.OwnerType == OwnerType.Client),
                ContactMembers = s.Memberships.Count(m => m.OwnerType == OwnerType.Contact)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (segmentStats == null)
            throw new InvalidOperationException($"Segment with ID {request.SegmentId} not found");

        // Parse rules count from JSON
        int rulesCount = 0;
        var segment = await _context.Segments
            .Where(s => s.Id == request.SegmentId.ToString())
            .Select(s => s.DefinitionJson)
            .FirstOrDefaultAsync(cancellationToken);

        if (!string.IsNullOrEmpty(segment))
        {
            try
            {
                var definition = Newtonsoft.Json.JsonConvert.DeserializeObject<Domain.Models.SegmentDefinition>(segment);
                rulesCount = CountRules(definition?.Rules);
            }
            catch
            {
                rulesCount = 0;
            }
        }

        // Calculate rebuild duration (placeholder - would be tracked in actual implementation)
        TimeSpan? rebuildDuration = segmentStats.LastRebuiltAt.HasValue
            ? TimeSpan.FromMilliseconds(new Random().Next(100, 5000)) // Placeholder
            : null;

        return new SegmentStatsDto
        {
            SegmentId = Guid.Parse(segmentStats.Id),
            SegmentName = segmentStats.Name,
            TotalMembers = segmentStats.TotalMembers,
            ClientMembers = segmentStats.ClientMembers,
            ContactMembers = segmentStats.ContactMembers,
            LastRebuiltAt = segmentStats.LastRebuiltAt,
            RebuildDuration = rebuildDuration,
            RulesCount = rulesCount,
            IsActive = segmentStats.IsActive
        };
    }

    /// <summary>
    /// Recursively count rules in a segment definition
    /// </summary>
    private static int CountRules(Domain.Models.SegmentRuleGroup? rules)
    {
        if (rules == null) return 0;

        int count = 0;

        if (rules.All != null)
            count += rules.All.Sum(CountRules);

        if (rules.Any != null)
            count += rules.Any.Sum(CountRules);

        if (rules.Not != null)
            count += rules.Not.Sum(CountRules);

        return count;
    }

    /// <summary>
    /// Count rules in a base rule (handles both conditions and groups)
    /// </summary>
    private static int CountRules(Domain.Models.SegmentRuleBase rule)
    {
        return rule switch
        {
            Domain.Models.SegmentCondition => 1,
            Domain.Models.SegmentRuleGroup group => CountRules(group),
            _ => 0
        };
    }
}