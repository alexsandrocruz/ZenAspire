// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Common.Interfaces;
using CleanAspire.Application.Common.Models;
using CleanAspire.Application.Features.Interactions.DTOs;
using CleanAspire.Application.Pipeline;
using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Enums;

namespace CleanAspire.Application.Features.Interactions.Queries;

/// <summary>
/// Query for getting interactions for a specific owner (Client, Contact, etc.)
/// with filtering and pagination support.
/// </summary>
public record GetInteractionsByOwnerQuery : IFusionCacheRequest<PaginatedResult<InteractionListDto>>
{
    /// <summary>
    /// Type of entity (Client, Contact, etc.)
    /// </summary>
    public OwnerType OwnerType { get; init; }

    /// <summary>
    /// ID of the entity
    /// </summary>
    public Guid OwnerId { get; init; }

    /// <summary>
    /// Filter by interaction type (optional)
    /// </summary>
    public InteractionType? Type { get; init; }

    /// <summary>
    /// Filter by direction (optional)
    /// </summary>
    public string? Direction { get; init; }

    /// <summary>
    /// Filter by start date (optional)
    /// </summary>
    public DateTime? StartDate { get; init; }

    /// <summary>
    /// Filter by end date (optional)
    /// </summary>
    public DateTime? EndDate { get; init; }

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; init; } = 10;

    /// <summary>
    /// Tags for cache invalidation
    /// </summary>
    public IEnumerable<string>? Tags => new[] { "interactions" };
}

/// <summary>
/// Handler for processing GetInteractionsByOwnerQuery.
/// Retrieves paginated list of interactions with filtering.
/// </summary>
public class GetInteractionsByOwnerQueryHandler : IRequestHandler<GetInteractionsByOwnerQuery, PaginatedResult<InteractionListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetInteractionsByOwnerQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<PaginatedResult<InteractionListDto>> Handle(GetInteractionsByOwnerQuery request, CancellationToken cancellationToken)
    {
        // Validate tenant
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            throw new UnauthorizedAccessException("TenantId is required");

        // Build query with filters
        var query = _context.Interactions
            .Where(i => i.TenantId == _currentUser.TenantId)
            .Where(i => i.OwnerType == request.OwnerType && i.OwnerId == request.OwnerId);

        // Apply type filter
        if (request.Type.HasValue)
        {
            query = query.Where(i => i.Type == request.Type.Value);
        }

        // Apply direction filter
        if (!string.IsNullOrEmpty(request.Direction))
        {
            query = query.Where(i => i.Direction == request.Direction);
        }

        // Apply date range filter
        if (request.StartDate.HasValue)
        {
            query = query.Where(i => i.At >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(i => i.At <= request.EndDate.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and ordering (most recent first)
        var interactions = await query
            .OrderByDescending(i => i.At)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new InteractionListDto
            {
                Id = i.Id,
                Type = (int)i.Type,
                TypeName = i.Type.ToString(),
                Direction = i.Direction,
                At = i.At,
                OwnerType = (int)i.OwnerType,
                OwnerTypeName = i.OwnerType.ToString(),
                OwnerId = i.OwnerId.ToString(),
                Subject = i.Subject,
                Snippet = i.Snippet,
                DurationSeconds = i.DurationSeconds,
                HandledByUserId = i.HandledByUserId,
                Sentiment = i.Sentiment,
                Created = i.Created,
                IsInbound = i.IsInbound,
                IsOutbound = i.IsOutbound
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResult<InteractionListDto>(
            interactions,
            totalCount,
            request.PageNumber,
            request.PageSize);
    }
}
