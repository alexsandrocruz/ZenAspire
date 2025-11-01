// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Features.Addresses.DTOs;
using CleanAspire.Application.Pipeline;
using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Enums;
using CleanAspire.Domain.Events;

namespace CleanAspire.Application.Features.Addresses.Commands;

/// <summary>
/// Command to create a new address for a Client or Contact
/// </summary>
public record CreateAddressCommand : IFusionCacheRefreshRequest<AddressDto>, IRequiresValidation
{
    public OwnerType OwnerType { get; init; }
    public string OwnerId { get; init; } = string.Empty;
    public string Line1 { get; init; } = string.Empty;
    public string? Line2 { get; init; }
    public string? District { get; init; }
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string Zip { get; init; } = string.Empty;
    public string Country { get; init; } = "BR";
    public decimal? GeoLat { get; init; }
    public decimal? GeoLng { get; init; }
    public bool IsPrimary { get; init; }
    public string? Label { get; init; }

    public IEnumerable<string>? Tags => new[] { "addresses" };
}

internal sealed class CreateAddressCommandHandler : IRequestHandler<CreateAddressCommand, AddressDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateAddressCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<AddressDto> Handle(CreateAddressCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            throw new UnauthorizedAccessException("TenantId is required");

        // If setting as primary, unset other primary addresses for this owner
        if (request.IsPrimary)
        {
            var existingPrimary = await _context.Addresses
                .Where(a => a.OwnerType == request.OwnerType &&
                           a.OwnerId == request.OwnerId &&
                           a.IsPrimary)
                .ToListAsync(cancellationToken);

            foreach (var addr in existingPrimary)
            {
                addr.IsPrimary = false;
            }
        }

        var entity = new Address
        {
            TenantId = _currentUser.TenantId,
            OwnerType = request.OwnerType,
            OwnerId = request.OwnerId,
            Line1 = request.Line1,
            Line2 = request.Line2,
            District = request.District,
            City = request.City,
            State = request.State,
            Zip = request.Zip,
            Country = request.Country,
            GeoLat = request.GeoLat,
            GeoLng = request.GeoLng,
            IsPrimary = request.IsPrimary,
            Label = request.Label
        };

        entity.AddDomainEvent(new AddressCreatedEvent(entity));

        _context.Addresses.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new AddressDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            OwnerType = entity.OwnerType,
            OwnerId = entity.OwnerId,
            Line1 = entity.Line1,
            Line2 = entity.Line2,
            District = entity.District,
            City = entity.City,
            State = entity.State,
            Zip = entity.Zip,
            Country = entity.Country,
            GeoLat = entity.GeoLat,
            GeoLng = entity.GeoLng,
            IsPrimary = entity.IsPrimary,
            Label = entity.Label,
            FullAddress = entity.FullAddress,
            Created = entity.Created,
            LastModified = entity.LastModified
        };
    }
}
