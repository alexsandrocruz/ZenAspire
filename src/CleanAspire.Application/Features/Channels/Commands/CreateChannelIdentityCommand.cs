// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Features.Channels.DTOs;
using CleanAspire.Application.Pipeline;
using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Enums;
using CleanAspire.Domain.Events;

namespace CleanAspire.Application.Features.Channels.Commands;

/// <summary>
/// Command to create a new channel identity for a Client or Contact
/// </summary>
public record CreateChannelIdentityCommand : IFusionCacheRefreshRequest<ChannelIdentityDto>, IRequiresValidation
{
    public OwnerType OwnerType { get; init; }
    public string OwnerId { get; init; } = string.Empty;
    public ChannelType Type { get; init; }
    public string Value { get; init; } = string.Empty;
    public bool IsPrimary { get; init; }
    public string? Label { get; init; }
    public bool OptedIn { get; init; } = true;

    public IEnumerable<string>? Tags => new[] { "channels" };
}

internal sealed class CreateChannelIdentityCommandHandler : IRequestHandler<CreateChannelIdentityCommand, ChannelIdentityDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateChannelIdentityCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<ChannelIdentityDto> Handle(CreateChannelIdentityCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            throw new UnauthorizedAccessException("TenantId is required");

        // Check for duplicate channel value for the same owner and type
        var exists = await _context.ChannelIdentities
            .AnyAsync(c => c.OwnerType == request.OwnerType &&
                          c.OwnerId == request.OwnerId &&
                          c.Type == request.Type &&
                          c.Value == request.Value,
                     cancellationToken);

        if (exists)
            throw new InvalidOperationException($"Channel {request.Type} with value '{request.Value}' already exists for this owner");

        // If setting as primary, unset other primary channels of the same type for this owner
        if (request.IsPrimary)
        {
            var existingPrimary = await _context.ChannelIdentities
                .Where(c => c.OwnerType == request.OwnerType &&
                           c.OwnerId == request.OwnerId &&
                           c.Type == request.Type &&
                           c.IsPrimary)
                .ToListAsync(cancellationToken);

            foreach (var channel in existingPrimary)
            {
                channel.IsPrimary = false;
            }
        }

        var entity = new ChannelIdentity
        {
            TenantId = _currentUser.TenantId,
            OwnerType = request.OwnerType,
            OwnerId = request.OwnerId,
            Type = request.Type,
            Value = request.Value.Trim(),
            IsPrimary = request.IsPrimary,
            Label = request.Label,
            OptedIn = request.OptedIn,
            OptedInAt = request.OptedIn ? DateTime.UtcNow : null
        };

        entity.AddDomainEvent(new ChannelIdentityCreatedEvent(entity));

        _context.ChannelIdentities.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new ChannelIdentityDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            OwnerType = entity.OwnerType,
            OwnerId = entity.OwnerId,
            Type = entity.Type,
            Value = entity.Value,
            IsPrimary = entity.IsPrimary,
            VerifiedAt = entity.VerifiedAt,
            IsVerified = entity.IsVerified,
            Label = entity.Label,
            OptedIn = entity.OptedIn,
            OptedInAt = entity.OptedInAt,
            DisplayValue = entity.DisplayValue,
            Created = entity.Created,
            LastModified = entity.LastModified
        };
    }
}
