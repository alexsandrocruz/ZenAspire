// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Pipeline;
using CleanAspire.Domain.Events;

namespace CleanAspire.Application.Features.Channels.Commands;

/// <summary>
/// Command to update an existing channel identity
/// </summary>
public record UpdateChannelIdentityCommand : IFusionCacheRefreshRequest<Unit>, IRequiresValidation
{
    public string Id { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string? Label { get; init; }

    public IEnumerable<string>? Tags => new[] { "channels" };
}

internal sealed class UpdateChannelIdentityCommandHandler : IRequestHandler<UpdateChannelIdentityCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public UpdateChannelIdentityCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Unit> Handle(UpdateChannelIdentityCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.ChannelIdentities
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new KeyNotFoundException($"ChannelIdentity with ID {request.Id} not found");

        entity.Value = request.Value.Trim();
        entity.Label = request.Label;

        entity.AddDomainEvent(new ChannelIdentityUpdatedEvent(entity));

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
