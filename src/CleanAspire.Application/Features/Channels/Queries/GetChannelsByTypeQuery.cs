// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Features.Channels.DTOs;
using CleanAspire.Domain.Enums;

namespace CleanAspire.Application.Features.Channels.Queries;

/// <summary>
/// Query to get channel identities by owner and channel type
/// </summary>
public record GetChannelsByTypeQuery : IFusionCacheRequest<List<ChannelIdentityDto>>
{
    public OwnerType OwnerType { get; init; }
    public string OwnerId { get; init; } = string.Empty;
    public ChannelType ChannelType { get; init; }

    public string CacheKey => $"channels_{OwnerType}_{OwnerId}_{ChannelType}";
    public IEnumerable<string>? Tags => new[] { "channels" };
}

internal sealed class GetChannelsByTypeQueryHandler : IRequestHandler<GetChannelsByTypeQuery, List<ChannelIdentityDto>>
{
    private readonly IApplicationDbContext _context;

    public GetChannelsByTypeQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<List<ChannelIdentityDto>> Handle(GetChannelsByTypeQuery request, CancellationToken cancellationToken)
    {
        var channels = await _context.ChannelIdentities
            .Where(c => c.OwnerType == request.OwnerType &&
                       c.OwnerId == request.OwnerId &&
                       c.Type == request.ChannelType)
            .OrderByDescending(c => c.IsPrimary)
            .ThenBy(c => c.Label)
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
