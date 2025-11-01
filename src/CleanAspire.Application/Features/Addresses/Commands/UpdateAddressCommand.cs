// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Pipeline;
using CleanAspire.Domain.Events;

namespace CleanAspire.Application.Features.Addresses.Commands;

/// <summary>
/// Command to update an existing address
/// </summary>
public record UpdateAddressCommand : IFusionCacheRefreshRequest<Unit>, IRequiresValidation
{
    public string Id { get; init; } = string.Empty;
    public string Line1 { get; init; } = string.Empty;
    public string? Line2 { get; init; }
    public string? District { get; init; }
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string Zip { get; init; } = string.Empty;
    public string Country { get; init; } = "BR";
    public decimal? GeoLat { get; init; }
    public decimal? GeoLng { get; init; }
    public string? Label { get; init; }

    public IEnumerable<string>? Tags => new[] { "addresses" };
}

internal sealed class UpdateAddressCommandHandler : IRequestHandler<UpdateAddressCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public UpdateAddressCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Unit> Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new KeyNotFoundException($"Address with ID {request.Id} not found");

        entity.Line1 = request.Line1;
        entity.Line2 = request.Line2;
        entity.District = request.District;
        entity.City = request.City;
        entity.State = request.State;
        entity.Zip = request.Zip;
        entity.Country = request.Country;
        entity.GeoLat = request.GeoLat;
        entity.GeoLng = request.GeoLng;
        entity.Label = request.Label;

        entity.AddDomainEvent(new AddressUpdatedEvent(entity));

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
