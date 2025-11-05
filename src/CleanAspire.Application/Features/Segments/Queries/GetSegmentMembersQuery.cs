using CleanAspire.Application.Features.Segments.DTOs;
using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Enums;
using CleanAspire.Application.Pipeline;
using Microsoft.EntityFrameworkCore;

namespace CleanAspire.Application.Features.Segments.Queries;

/// <summary>
/// Query to fetch segment members with pagination and filtering.
/// </summary>
public record GetSegmentMembersQuery(
    Guid SegmentId,
    OwnerType? OwnerType = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = "ComputedAt",
    bool SortDescending = false
) : IRequest<(List<SegmentMembershipDto> Members, int TotalCount)>;

/// <summary>
/// Handler for the GetSegmentMembersQuery.
/// Retrieves segment members with pagination and filtering.
/// </summary>
public class GetSegmentMembersQueryHandler : IRequestHandler<GetSegmentMembersQuery, (List<SegmentMembershipDto> Members, int TotalCount)>
{
    private readonly IApplicationDbContext _context;

    public GetSegmentMembersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<(List<SegmentMembershipDto> Members, int TotalCount)> Handle(
        GetSegmentMembersQuery request, CancellationToken cancellationToken)
    {
        // Base query
        var query = _context.SegmentMemberships
            .Where(sm => sm.SegmentId == request.SegmentId);

        // Filter by owner type
        if (request.OwnerType.HasValue)
        {
            query = query.Where(sm => sm.OwnerType == request.OwnerType.Value);
        }

        // Search functionality
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(sm =>
                sm.Segment.Name.Contains(request.Search) ||
                (sm.OwnerType == OwnerType.Client && sm.Segment.Memberships.Any(m => m.OwnerId.ToString().Contains(request.Search))) ||
                (sm.OwnerType == OwnerType.Contact && sm.Segment.Memberships.Any(m => m.OwnerId.ToString().Contains(request.Search))));
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Sorting
        query = request.SortBy?.ToLower() switch
        {
            "computedat" or "computed_at" => request.SortDescending
                ? query.OrderByDescending(sm => sm.ComputedAt)
                : query.OrderBy(sm => sm.ComputedAt),
            "ownertype" or "owner_type" => request.SortDescending
                ? query.OrderByDescending(sm => sm.OwnerType)
                : query.OrderBy(sm => sm.OwnerType),
            _ => request.SortDescending
                ? query.OrderByDescending(sm => sm.ComputedAt)
                : query.OrderBy(sm => sm.ComputedAt)
        };

        // Pagination
        var skip = (request.Page - 1) * request.PageSize;
        query = query.Skip(skip).Take(request.PageSize);

        // Execute query and map to DTOs
        var members = await query
            .Select(sm => new SegmentMembershipDto
            {
                SegmentId = sm.SegmentId,
                SegmentName = sm.Segment.Name,
                TenantId = sm.TenantId,
                OwnerType = sm.OwnerType,
                OwnerTypeDisplay = sm.OwnerType.ToString(),
                OwnerId = sm.OwnerId,
                OwnerName = sm.OwnerType == OwnerType.Client
                    ? _context.Clients
                        .Where(c => c.Id == sm.OwnerId.ToString())
                        .Select(c => c.Name)
                        .FirstOrDefault()
                    : sm.OwnerType == OwnerType.Contact
                        ? _context.Contacts
                            .Where(c => c.Id == sm.OwnerId.ToString())
                            .Select(c => $"{c.FirstName} {c.LastName}")
                            .FirstOrDefault()
                    : sm.OwnerId.ToString(),
                ComputedAt = sm.ComputedAt
            })
            .ToListAsync(cancellationToken);

        return (members, totalCount);
    }
}