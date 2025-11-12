using CleanAspire.Application.Features.Segments.DTOs;
using CleanAspire.Domain.Entities;
using CleanAspire.Application.Pipeline;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CleanAspire.Application.Features.Segments.Queries;

/// <summary>
/// Query to fetch a specific segment by its ID.
/// </summary>
public record GetSegmentByIdQuery(Guid Id) : IRequest<SegmentDto?>;

/// <summary>
/// Handler for the GetSegmentByIdQuery.
/// Retrieves a specific segment from the database and maps it to DTO.
/// </summary>
public class GetSegmentByIdQueryHandler : IRequestHandler<GetSegmentByIdQuery, SegmentDto?>
{
    private readonly IApplicationDbContext _context;

    public GetSegmentByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<SegmentDto?> Handle(GetSegmentByIdQuery request, CancellationToken cancellationToken)
    {
        var segment = await _context.Segments
            .Where(s => s.Id == request.Id.ToString())
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
            .FirstOrDefaultAsync(cancellationToken);

        return segment;
    }
}