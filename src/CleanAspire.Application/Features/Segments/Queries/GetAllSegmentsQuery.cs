using CleanAspire.Application.Features.Segments.DTOs;
using CleanAspire.Domain.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CleanAspire.Application.Features.Segments.Queries;

/// <summary>
/// Query to fetch all segments without pagination.
/// Useful for dropdowns, exports, and simple lists.
/// </summary>
public record GetAllSegmentsQuery : IFusionCacheRequest<List<SegmentDto>>
{
    /// <summary>
    /// Cache key for storing the result of this query.
    /// </summary>
    public string CacheKey => "all_segments";

    /// <summary>
    /// Tags for cache invalidation, categorizing this query under "segments".
    /// </summary>
    public IEnumerable<string>? Tags => new[] { "segments" };
}

/// <summary>
/// Handler for the GetAllSegmentsQuery.
/// Retrieves all segments from the database and maps them to DTOs.
/// </summary>
public class GetAllSegmentsQueryHandler : IRequestHandler<GetAllSegmentsQuery, List<SegmentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllSegmentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<List<SegmentDto>> Handle(GetAllSegmentsQuery request, CancellationToken cancellationToken)
    {
        var segments = await _context.Segments
            .OrderBy(s => s.Name)
            .Select(s => new SegmentDto
            {
                Id = s.Id.ToString(),
                TenantId = s.TenantId,
                Name = s.Name,
                Description = s.Description,
                DefinitionJson = s.DefinitionJson,
                IsActive = s.IsActive,
                LastRebuiltAt = s.LastRebuiltAt,
                MemberCount = s.MemberCount,
                TargetOwnerTypes = s.TargetOwnerTypes.ToList(),
                Created = s.Created,
                CreatedBy = s.CreatedBy,
                LastModified = s.LastModified,
                LastModifiedBy = s.LastModifiedBy
            })
            .ToListAsync(cancellationToken);

        return segments;
    }
}