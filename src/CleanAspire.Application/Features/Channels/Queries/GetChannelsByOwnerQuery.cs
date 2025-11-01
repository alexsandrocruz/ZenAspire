// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Features.Channels.DTOs;
using CleanAspire.Domain.Enums;

namespace CleanAspire.Application.Features.Channels.Queries;

/// <summary>
/// Query to get all channel identities for a specific owner (Client or Contact)
/// </summary>
public record GetChannelsByOwnerQuery : IFusionCacheRequest<List<ChannelIdentityDto>>
{
    public OwnerType OwnerType { get; init; }
    public string OwnerId { get; init; } = string.Empty;

    public string CacheKey => $"channels_owner_{OwnerType}_{OwnerId}";
    public IEnumerable<string>? Tags => new[] { "channels" };
}

internal sealed class GetChannelsByOwnerQueryHandler : IRequestHandler<GetChannelsByOwnerQuery, List<ChannelIdentityDto>>
{
    private readonly IApplicationDbContext _context;

    public GetChannelsByOwnerQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<List<ChannelIdentityDto>> Handle(GetChannelsByOwnerQuery request, CancellationToken cancellationToken)
    {
        var channels = await _context.ChannelIdentities
            .Where(c => c.OwnerType == request.OwnerType && c.OwnerId == request.OwnerId)
            .OrderBy(c => c.Type) // Group by channel type
            .ThenByDescending(c => c.IsPrimary) // Primary channels first within each type
            .ThenBy(c => c.Label)
            .ThenBy(c => c.Created)
            .Select(c => new ChannelIdentityDto
            {
                Id = c.Id,
                TenantId = c.TenantId,
                OwnerType = c.OwnerType,
                OwnerId = c.OwnerId,
                Type = c.Type,
                Value = c.Value,
                IsPrimary = c.IsPrimary,
                VerifiedAt = c.VerifiedAt,
                IsVerified = c.IsVerified,
                Label = c.Label,
                OptedIn = c.OptedIn,
                OptedInAt = c.OptedInAt,
                DisplayValue = c.DisplayValue,
                Created = c.Created,
                LastModified = c.LastModified
            })
            .ToListAsync(cancellationToken);

        return channels;
    }
}
