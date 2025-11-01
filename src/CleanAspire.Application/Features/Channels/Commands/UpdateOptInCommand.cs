// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Pipeline;
using CleanAspire.Domain.Events;

namespace CleanAspire.Application.Features.Channels.Commands;

/// <summary>
/// Command to update opt-in status for a channel (LGPD compliance)
/// </summary>
public record UpdateOptInCommand : IFusionCacheRefreshRequest<Unit>
{
    public string Id { get; init; } = string.Empty;
    public bool OptedIn { get; init; }

    public IEnumerable<string>? Tags => new[] { "channels" };
}

internal sealed class UpdateOptInCommandHandler : IRequestHandler<UpdateOptInCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public UpdateOptInCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Unit> Handle(UpdateOptInCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.ChannelIdentities
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new KeyNotFoundException($"ChannelIdentity with ID {request.Id} not found");

        var wasOptedIn = entity.OptedIn;
        entity.OptedIn = request.OptedIn;
        entity.OptedInAt = DateTime.UtcNow;

        entity.AddDomainEvent(new ChannelOptInChangedEvent(entity, wasOptedIn));

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
