// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Features.Addresses.DTOs;
using CleanAspire.Domain.Enums;

namespace CleanAspire.Application.Features.Addresses.Queries;

/// <summary>
/// Query to get all addresses for a specific owner (Client or Contact)
/// </summary>
public record GetAddressesByOwnerQuery : IFusionCacheRequest<List<AddressDto>>
{
    public OwnerType OwnerType { get; init; }
    public string OwnerId { get; init; } = string.Empty;

    public string CacheKey => $"addresses_owner_{OwnerType}_{OwnerId}";
    public IEnumerable<string>? Tags => new[] { "addresses" };
}

internal sealed class GetAddressesByOwnerQueryHandler : IRequestHandler<GetAddressesByOwnerQuery, List<AddressDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAddressesByOwnerQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<List<AddressDto>> Handle(GetAddressesByOwnerQuery request, CancellationToken cancellationToken)
    {
        var addresses = await _context.Addresses
            .Where(a => a.OwnerType == request.OwnerType && a.OwnerId == request.OwnerId)
            .OrderByDescending(a => a.IsPrimary) // Primary addresses first
            .ThenBy(a => a.Label)
            .ThenBy(a => a.Created)
            .Select(a => new AddressDto
            {
                Id = a.Id,
                TenantId = a.TenantId,
                OwnerType = a.OwnerType,
                OwnerId = a.OwnerId,
                Line1 = a.Line1,
                Line2 = a.Line2,
                District = a.District,
                City = a.City,
                State = a.State,
                Zip = a.Zip,
                Country = a.Country,
                GeoLat = a.GeoLat,
                GeoLng = a.GeoLng,
                IsPrimary = a.IsPrimary,
                Label = a.Label,
                FullAddress = a.FullAddress,
                Created = a.Created,
                LastModified = a.LastModified
            })
            .ToListAsync(cancellationToken);

        return addresses;
    }
}
