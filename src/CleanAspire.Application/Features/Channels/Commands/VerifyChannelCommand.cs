// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Pipeline;
using CleanAspire.Domain.Events;

namespace CleanAspire.Application.Features.Channels.Commands;

/// <summary>
/// Command to mark a channel as verified
/// </summary>
public record VerifyChannelCommand : IFusionCacheRefreshRequest<Unit>
{
    public string Id { get; init; } = string.Empty;

    public IEnumerable<string>? Tags => new[] { "channels" };
}

internal sealed class VerifyChannelCommandHandler : IRequestHandler<VerifyChannelCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public VerifyChannelCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Unit> Handle(VerifyChannelCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.ChannelIdentities
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new KeyNotFoundException($"ChannelIdentity with ID {request.Id} not found");

        if (entity.VerifiedAt.HasValue)
            return Unit.Value; // Already verified

        entity.VerifiedAt = DateTime.UtcNow;

        entity.AddDomainEvent(new ChannelIdentityVerifiedEvent(entity));

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
