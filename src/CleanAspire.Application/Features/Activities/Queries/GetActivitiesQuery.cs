// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Common.Interfaces.FusionCache;
using CleanAspire.Application.Features.Activities.DTOs;
using CleanAspire.Domain.Entities;

namespace CleanAspire.Application.Features.Activities.Queries;

/// <summary>
/// Query to fetch activities with pagination, filtering, and sorting options.
/// Supports filtering by status, assigned user, regarding type, and regarding ID.
/// Implements IFusionCacheRequest to enable caching for performance optimization.
/// </summary>
public record GetActivitiesQuery(
    string Keywords = "",
    int PageNumber = 0,
    int PageSize = 15,
    string OrderBy = "Start",
    string SortDirection = "Descending",
    ActivityStatus? FilterByStatus = null,
    ActivityType? FilterByType = null,
    ActivityPriority? FilterByPriority = null,
    string? FilterByAssignedTo = null,
    RegardingType? FilterByRegardingType = null,
    Guid? FilterByRegardingId = null,
    DateTime? FilterStartFrom = null,
    DateTime? FilterStartTo = null,
    DateTime? FilterDueFrom = null,
    DateTime? FilterDueTo = null,
    bool? FilterOverdueOnly = null
) : IFusionCacheRequest<PaginatedResult<ActivityListDto>>
{
    /// <summary>
    /// Tags for cache invalidation, categorizing this query under "activities".
    /// </summary>
    public IEnumerable<string>? Tags => new[] { "activities" };

    /// <summary>
    /// Cache key for storing the result of this query, unique to its parameters.
    /// </summary>
    public string CacheKey => $"activities_{Keywords}_{PageNumber}_{PageSize}_{OrderBy}_{SortDirection}_{FilterByStatus}_{FilterByType}_{FilterByPriority}_{FilterByAssignedTo}_{FilterByRegardingType}_{FilterByRegardingId}_{FilterStartFrom}_{FilterStartTo}_{FilterDueFrom}_{FilterDueTo}_{FilterOverdueOnly}";
}

/// <summary>
/// Handler for the GetActivitiesQuery.
/// Retrieves paginated, filtered, and sorted activity data from the database.
/// </summary>
public class GetActivitiesQueryHandler : IRequestHandler<GetActivitiesQuery, PaginatedResult<ActivityListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetActivitiesQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<PaginatedResult<ActivityListDto>> Handle(GetActivitiesQuery request, CancellationToken cancellationToken)
    {
        // Validate tenant
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            throw new UnauthorizedAccessException("TenantId is required");

        // Build the base query with tenant filter
        var query = _context.Activities
            .Where(a => a.TenantId == _currentUser.TenantId)
            .AsQueryable();

        // Apply status filter
        if (request.FilterByStatus.HasValue)
        {
            query = query.Where(x => x.Status == request.FilterByStatus.Value);
        }

        // Apply type filter
        if (request.FilterByType.HasValue)
        {
            query = query.Where(x => x.Type == request.FilterByType.Value);
        }

        // Apply priority filter
        if (request.FilterByPriority.HasValue)
        {
            query = query.Where(x => x.Priority == request.FilterByPriority.Value);
        }

        // Apply assigned to filter
        if (!string.IsNullOrEmpty(request.FilterByAssignedTo))
        {
            query = query.Where(x => x.AssignedToUserId == request.FilterByAssignedTo);
        }

        // Apply regarding type filter
        if (request.FilterByRegardingType.HasValue)
        {
            query = query.Where(x => x.RegardingType == request.FilterByRegardingType.Value);
        }

        // Apply regarding ID filter
        if (request.FilterByRegardingId.HasValue)
        {
            query = query.Where(x => x.RegardingId == request.FilterByRegardingId.Value);
        }

        // Apply start date range filter
        if (request.FilterStartFrom.HasValue)
        {
            query = query.Where(x => x.Start >= request.FilterStartFrom.Value);
        }
        if (request.FilterStartTo.HasValue)
        {
            query = query.Where(x => x.Start <= request.FilterStartTo.Value);
        }

        // Apply due date range filter
        if (request.FilterDueFrom.HasValue)
        {
            query = query.Where(x => x.Due.HasValue && x.Due.Value >= request.FilterDueFrom.Value);
        }
        if (request.FilterDueTo.HasValue)
        {
            query = query.Where(x => x.Due.HasValue && x.Due.Value <= request.FilterDueTo.Value);
        }

        // Apply overdue filter
        if (request.FilterOverdueOnly.HasValue && request.FilterOverdueOnly.Value)
        {
            var now = DateTime.UtcNow;
            query = query.Where(x => x.Status == ActivityStatus.Open && x.Due.HasValue && x.Due.Value < now);
        }

        // Apply keyword search
        if (!string.IsNullOrEmpty(request.Keywords))
        {
            var keywords = request.Keywords.ToLower();
            query = query.Where(x =>
                x.Subject.ToLower().Contains(keywords) ||
                (x.Description != null && x.Description.ToLower().Contains(keywords)) ||
                (x.Location != null && x.Location.ToLower().Contains(keywords))
            );
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
