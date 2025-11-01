// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Common.Interfaces;
using CleanAspire.Application.Pipeline;
using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Enums;

namespace CleanAspire.Application.Features.Tags.Commands;

/// <summary>
/// Command for linking a tag to an entity (Client, Contact, etc.).
/// </summary>
public record LinkTagCommand : IFusionCacheRefreshRequest<Unit>, IRequiresValidation
{
    public string TagId { get; init; } = string.Empty;
    public OwnerType OwnerType { get; init; }
    public string OwnerId { get; init; } = string.Empty;

    public IEnumerable<string>? Tags => new[] { "tags", "taglinks" };
}

/// <summary>
/// Handler for processing LinkTagCommand.
/// </summary>
public class LinkTagCommandHandler : IRequestHandler<LinkTagCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public LinkTagCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(LinkTagCommand request, CancellationToken cancellationToken)
    {
        // Validate tenant
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            throw new UnauthorizedAccessException("TenantId is required");

        // Verify tag exists
        var tagExists = await _context.Tags
            .AnyAsync(t => t.Id == request.TagId, cancellationToken);

        if (!tagExists)
            throw new KeyNotFoundException($"Tag with Id '{request.TagId}' was not found.");

        // Check if link already exists
        var linkExists = await _context.TagLinks
            .AnyAsync(tl => tl.TagId == request.TagId
                && tl.OwnerType == request.OwnerType
                && tl.OwnerId == request.OwnerId, cancellationToken);

        if (linkExists)
            return Unit.Value; // Already linked, idempotent operation

        var tagLink = new TagLink
        {
            TenantId = _currentUser.TenantId,
            TagId = request.TagId,
            OwnerType = request.OwnerType,
            OwnerId = request.OwnerId,
            LinkedAt = DateTime.UtcNow,
            LinkedBy = _currentUser.UserId
        };

        _context.TagLinks.Add(tagLink);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
