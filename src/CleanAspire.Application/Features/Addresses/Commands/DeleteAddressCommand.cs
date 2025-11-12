// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Pipeline;
using CleanAspire.Domain.Events;

namespace CleanAspire.Application.Features.Addresses.Commands;

/// <summary>
/// Command to delete an address
/// </summary>
public record DeleteAddressCommand : IFusionCacheRefreshRequest<Unit>
{
    public string Id { get; init; } = string.Empty;

    public IEnumerable<string>? Tags => new[] { "addresses" };
}

internal sealed class DeleteAddressCommandHandler : IRequestHandler<DeleteAddressCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteAddressCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Unit> Handle(DeleteAddressCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new KeyNotFoundException($"Address with ID {request.Id} not found");

        entity.AddDomainEvent(new AddressDeletedEvent(entity));

        _context.Addresses.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
