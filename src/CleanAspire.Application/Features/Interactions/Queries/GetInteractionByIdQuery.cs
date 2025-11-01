// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Common.Interfaces;
using CleanAspire.Application.Features.Interactions.DTOs;
using CleanAspire.Application.Pipeline;

namespace CleanAspire.Application.Features.Interactions.Queries;

/// <summary>
/// Query for getting a single interaction by its ID.
/// </summary>
public record GetInteractionByIdQuery(string Id) : IFusionCacheRequest<InteractionDto?>
{
    public IEnumerable<string>? Tags => new[] { "interactions" };
}

/// <summary>
/// Handler for processing GetInteractionByIdQuery.
/// Retrieves a single interaction with all details.
/// </summary>
public class GetInteractionByIdQueryHandler : IRequestHandler<GetInteractionByIdQuery, InteractionDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetInteractionByIdQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<InteractionDto?> Handle(GetInteractionByIdQuery request, CancellationToken cancellationToken)
    {
        // Validate tenant
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            throw new UnauthorizedAccessException("TenantId is required");

        var interaction = await _context.Interactions
            .Where(i => i.Id == request.Id && i.TenantId == _currentUser.TenantId)
            .Select(i => new InteractionDto
            {
                Id = i.Id,
                Type = (int)i.Type,
                TypeName = i.Type.ToString(),
                Direction = i.Direction,
                At = i.At,
                OwnerType = (int)i.OwnerType,
                OwnerTypeName = i.OwnerType.ToString(),
                OwnerId = i.OwnerId.ToString(),
                ChannelRef = i.ChannelRef,
                Subject = i.Subject,
                Snippet = i.Snippet,
                PayloadJson = i.PayloadJson,
                DurationSeconds = i.DurationSeconds,
                HandledByUserId = i.HandledByUserId,
                Sentiment = i.Sentiment,
                Tags = i.Tags,
                Created = i.Created,
                CreatedBy = i.CreatedBy,
                LastModified = i.LastModified,
                LastModifiedBy = i.LastModifiedBy,
                IsInbound = i.IsInbound,
                IsOutbound = i.IsOutbound
            })
            .FirstOrDefaultAsync(cancellationToken);

        return interaction;
    }
}
