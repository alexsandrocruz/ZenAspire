// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Pipeline;
using CleanAspire.Domain.Events;

namespace CleanAspire.Application.Features.Channels.Commands;

/// <summary>
/// Command to delete a channel identity
/// </summary>
public record DeleteChannelIdentityCommand : IFusionCacheRefreshRequest<Unit>
{
    public string Id { get; init; } = string.Empty;

    public IEnumerable<string>? Tags => new[] { "channels" };
}

internal sealed class DeleteChannelIdentityCommandHandler : IRequestHandler<DeleteChannelIdentityCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteChannelIdentityCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Unit> Handle(DeleteChannelIdentityCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.ChannelIdentities
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new KeyNotFoundException($"ChannelIdentity with ID {request.Id} not found");

        entity.AddDomainEvent(new ChannelIdentityDeletedEvent(entity));

        _context.ChannelIdentities.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
