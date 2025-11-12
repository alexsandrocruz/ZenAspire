// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Features.Addresses.DTOs;

namespace CleanAspire.Application.Features.Addresses.Queries;

/// <summary>
/// Query to get a single address by ID
/// </summary>
public record GetAddressByIdQuery(string Id) : IFusionCacheRequest<AddressDto?>
{
    public string CacheKey => $"address_{Id}";
    public IEnumerable<string>? Tags => new[] { "addresses" };
}

internal sealed class GetAddressByIdQueryHandler : IRequestHandler<GetAddressByIdQuery, AddressDto?>
{
    private readonly IApplicationDbContext _context;

    public GetAddressByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<AddressDto?> Handle(GetAddressByIdQuery request, CancellationToken cancellationToken)
    {
        var address = await _context.Addresses
            .Where(a => a.Id == request.Id)
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
