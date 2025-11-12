// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Common.Interfaces.FusionCache;
using CleanAspire.Application.Features.Activities.DTOs;
using CleanAspire.Domain.Entities;

namespace CleanAspire.Application.Features.Activities.Queries;

/// <summary>
/// Query to fetch activities assigned to a specific user.
/// Useful for personal activity lists and dashboards.
/// Implements IFusionCacheRequest to enable caching for performance optimization.
/// </summary>
public record GetActivitiesByUserQuery(
    string? UserId = null, // If null, uses current user
    ActivityStatus? FilterByStatus = null,
    bool IncludeOverdueOnly = false,
    int PageNumber = 0,
    int PageSize = 15,
    string OrderBy = "Start",
    string SortDirection = "Ascending"
) : IFusionCacheRequest<PaginatedResult<ActivityListDto>>
{
    /// <summary>
    /// Tags for cache invalidation, categorizing this query under "activities".
    /// </summary>
    public IEnumerable<string>? Tags => new[] { "activities" };

    /// <summary>
    /// Cache key for storing the result of this query, unique to its parameters.
    /// </summary>
    public string CacheKey => $"activitiesbyuser_{UserId}_{FilterByStatus}_{IncludeOverdueOnly}_{PageNumber}_{PageSize}_{OrderBy}_{SortDirection}";
}

/// <summary>
/// Handler for the GetActivitiesByUserQuery.
/// Retrieves paginated and filtered activity data for a specific user.
/// </summary>
public class GetActivitiesByUserQueryHandler : IRequestHandler<GetActivitiesByUserQuery, PaginatedResult<ActivityListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetActivitiesByUserQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<PaginatedResult<ActivityListDto>> Handle(GetActivitiesByUserQuery request, CancellationToken cancellationToken)
    {
        // Validate tenant
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            throw new UnauthorizedAccessException("TenantId is required");

        // Use provided UserId or default to current user
        var userId = request.UserId ?? _currentUser.UserId;

        if (string.IsNullOrEmpty(userId))
            throw new ArgumentException("UserId is required");

        // Build the base query with tenant and user filters
        var query = _context.Activities
            .Where(a => a.TenantId == _currentUser.TenantId && a.AssignedToUserId == userId)
            .AsQueryable();

        // Apply status filter
        if (request.FilterByStatus.HasValue)
        {
            query = query.Where(x => x.Status == request.FilterByStatus.Value);
        }

        // Apply overdue filter
        if (request.IncludeOverdueOnly)
        {
            var now = DateTime.UtcNow;
            query = query.Where(x => x.Status == ActivityStatus.Open && x.Due.HasValue && x.Due.Value < now);
        }

        // Retrieve and paginate data with mapping
        var data = await query
            .OrderBy(request.OrderBy, request.SortDirection)
            .ProjectToPaginatedDataAsync(
                condition: x => true, // Already filtered above
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                mapperFunc: a => new ActivityListDto
                {
                    Id = a.Id,
                    Type = (int)a.Type,
                    TypeName = a.Type.ToString(),
                    Status = (int)a.Status,
                    StatusName = a.Status.ToString(),
                    Subject = a.Subject,
                    Start = a.Start,
                    Due = a.Due,
                    Priority = (int)a.Priority,
                    PriorityName = a.Priority.ToString(),
                    RegardingType = a.RegardingType.HasValue ? (int)a.RegardingType.Value : null,
                    RegardingTypeName = a.RegardingType.HasValue ? a.RegardingType.Value.ToString() : null,
                    RegardingId = a.RegardingId,
                    AssignedToUserId = a.AssignedToUserId,
                    CompletedAt = a.CompletedAt,
                    IsOverdue = a.IsOverdue,
                    IsCompleted = a.IsCompleted
                },
                cancellationToken: cancellationToken);

        return data;
    }
}
