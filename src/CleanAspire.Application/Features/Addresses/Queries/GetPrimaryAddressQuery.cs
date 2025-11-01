// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Features.Addresses.DTOs;
using CleanAspire.Domain.Enums;

namespace CleanAspire.Application.Features.Addresses.Queries;

/// <summary>
/// Query to get the primary address for an owner
/// </summary>
public record GetPrimaryAddressQuery : IFusionCacheRequest<AddressDto?>
{
    public OwnerType OwnerType { get; init; }
    public string OwnerId { get; init; } = string.Empty;

    public string CacheKey => $"address_primary_{OwnerType}_{OwnerId}";
    public IEnumerable<string>? Tags => new[] { "addresses" };
}

internal sealed class GetPrimaryAddressQueryHandler : IRequestHandler<GetPrimaryAddressQuery, AddressDto?>
{
    private readonly IApplicationDbContext _context;

    public GetPrimaryAddressQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<AddressDto?> Handle(GetPrimaryAddressQuery request, CancellationToken cancellationToken)
    {
        var address = await _context.Addresses
            .Where(a => a.OwnerType == request.OwnerType &&
                       a.OwnerId == request.OwnerId &&
                       a.IsPrimary)
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
            .FirstOrDefaultAsync(cancellationToken);

        return address;
    }
}
