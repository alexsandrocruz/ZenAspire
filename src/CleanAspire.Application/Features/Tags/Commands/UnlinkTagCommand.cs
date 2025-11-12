// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Common.Interfaces;
using CleanAspire.Application.Pipeline;
using CleanAspire.Domain.Enums;

namespace CleanAspire.Application.Features.Tags.Commands;

/// <summary>
/// Command for unlinking a tag from an entity.
/// </summary>
public record UnlinkTagCommand : IFusionCacheRefreshRequest<Unit>, IRequiresValidation
{
    public string TagId { get; init; } = string.Empty;
    public OwnerType OwnerType { get; init; }
    public string OwnerId { get; init; } = string.Empty;

    public IEnumerable<string>? Tags => new[] { "tags", "taglinks" };
}

/// <summary>
/// Handler for processing UnlinkTagCommand.
/// </summary>
public class UnlinkTagCommandHandler : IRequestHandler<UnlinkTagCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UnlinkTagCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(UnlinkTagCommand request, CancellationToken cancellationToken)
    {
        // Validate tenant
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            throw new UnauthorizedAccessException("TenantId is required");

        var tagLink = await _context.TagLinks
            .FirstOrDefaultAsync(tl => tl.TagId == request.TagId
                && tl.OwnerType == request.OwnerType
                && tl.OwnerId == request.OwnerId, cancellationToken);

        if (tagLink == null)
            return Unit.Value; // Already unlinked, idempotent operation

        _context.TagLinks.Remove(tagLink);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
