// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Features.Channels.DTOs;

namespace CleanAspire.Application.Features.Channels.Queries;

/// <summary>
/// Query to get a single channel identity by ID
/// </summary>
public record GetChannelByIdQuery(string Id) : IFusionCacheRequest<ChannelIdentityDto?>
{
    public string CacheKey => $"channel_{Id}";
    public IEnumerable<string>? Tags => new[] { "channels" };
}

internal sealed class GetChannelByIdQueryHandler : IRequestHandler<GetChannelByIdQuery, ChannelIdentityDto?>
{
    private readonly IApplicationDbContext _context;

    public GetChannelByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<ChannelIdentityDto?> Handle(GetChannelByIdQuery request, CancellationToken cancellationToken)
    {
        var channel = await _context.ChannelIdentities
            .Where(c => c.Id == request.Id)
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
            .FirstOrDefaultAsync(cancellationToken);

        return channel;
    }
}
