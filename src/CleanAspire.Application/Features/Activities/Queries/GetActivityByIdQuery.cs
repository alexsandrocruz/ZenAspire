// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Common.Interfaces.FusionCache;
using CleanAspire.Application.Features.Activities.DTOs;

namespace CleanAspire.Application.Features.Activities.Queries;

/// <summary>
/// Query to fetch a single activity by its ID.
/// Implements IFusionCacheRequest to enable caching.
/// </summary>
public record GetActivityByIdQuery(string Id) : IFusionCacheRequest<ActivityDto?>
{
    /// <summary>
    /// Cache key for storing the result of this query, specific to the activity ID.
    /// </summary>
    public string CacheKey => $"activity_{Id}";

    /// <summary>
    /// Tags for cache invalidation, categorizing this query under "activities".
    /// </summary>
    public IEnumerable<string>? Tags => new[] { "activities" };
}

/// <summary>
/// Handler for the GetActivityByIdQuery.
/// Fetches a single ActivityDto by its ID from the database.
/// </summary>
public class GetActivityByIdQueryHandler : IRequestHandler<GetActivityByIdQuery, ActivityDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetActivityByIdQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<ActivityDto?> Handle(GetActivityByIdQuery request, CancellationToken cancellationToken)
    {
        // Validate tenant
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            throw new UnauthorizedAccessException("TenantId is required");

        var activity = await _context.Activities
            .Where(a => a.Id == request.Id && a.TenantId == _currentUser.TenantId)
            .Select(a => new ActivityDto
            {
                Id = a.Id,
                Type = (int)a.Type,
                TypeName = a.Type.ToString(),
                Status = (int)a.Status,
                StatusName = a.Status.ToString(),
                Start = a.Start,
                Due = a.Due,
                Subject = a.Subject,
                Description = a.Description,
                RegardingType = a.RegardingType.HasValue ? (int)a.RegardingType.Value : null,
                RegardingTypeName = a.RegardingType.HasValue ? a.RegardingType.Value.ToString() : null,
                RegardingId = a.RegardingId,
                AssignedToUserId = a.AssignedToUserId,
                ReminderAt = a.ReminderAt,
                CompletedAt = a.CompletedAt,
                Priority = (int)a.Priority,
                PriorityName = a.Priority.ToString(),
                Location = a.Location,
                DurationMinutes = a.DurationMinutes,
                Created = a.Created,
                CreatedBy = a.CreatedBy,
                LastModified = a.LastModified,
                LastModifiedBy = a.LastModifiedBy,
                IsOverdue = a.IsOverdue,
                IsCompleted = a.IsCompleted,
                IsCanceled = a.IsCanceled
            })
            .SingleOrDefaultAsync(cancellationToken);

        return activity;
    }
}
