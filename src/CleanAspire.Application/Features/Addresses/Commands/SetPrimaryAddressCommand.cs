// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Pipeline;
using CleanAspire.Domain.Events;

namespace CleanAspire.Application.Features.Addresses.Commands;

/// <summary>
/// Command to set an address as primary for its owner
/// </summary>
public record SetPrimaryAddressCommand : IFusionCacheRefreshRequest<Unit>
{
    public string Id { get; init; } = string.Empty;

    public IEnumerable<string>? Tags => new[] { "addresses" };
}

internal sealed class SetPrimaryAddressCommandHandler : IRequestHandler<SetPrimaryAddressCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public SetPrimaryAddressCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Unit> Handle(SetPrimaryAddressCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new KeyNotFoundException($"Address with ID {request.Id} not found");

        // Unset primary flag on other addresses for the same owner
        var otherAddresses = await _context.Addresses
            .Where(a => a.OwnerType == entity.OwnerType &&
                       a.OwnerId == entity.OwnerId &&
                       a.Id != entity.Id &&
                       a.IsPrimary)
            .ToListAsync(cancellationToken);

        foreach (var addr in otherAddresses)
        {
            addr.IsPrimary = false;
        }

        var wasPrimary = entity.IsPrimary;
        entity.IsPrimary = true;

        entity.AddDomainEvent(new AddressPrimaryChangedEvent(entity, wasPrimary));

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
