// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Common.Interfaces.FusionCache;
using CleanAspire.Application.Pipeline;

namespace CleanAspire.Application.Features.Activities.Commands;

/// <summary>
/// Command for deleting an activity.
/// Performs a hard delete (permanent removal) from the database.
/// </summary>
public record DeleteActivityCommand : IFusionCacheRefreshRequest<Unit>, IRequiresValidation
{
    /// <summary>
    /// Unique identifier of the activity to delete
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Tags for cache invalidation
    /// </summary>
    public IEnumerable<string>? Tags => new[] { "activities" };
}

/// <summary>
/// Handler for processing DeleteActivityCommand.
/// Permanently removes an activity from the database.
/// </summary>
public class DeleteActivityCommandHandler : IRequestHandler<DeleteActivityCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteActivityCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(DeleteActivityCommand request, CancellationToken cancellationToken)
    {
        // Validate tenant
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            throw new UnauthorizedAccessException("TenantId is required");

        var activity = await _context.Activities
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.TenantId == _currentUser.TenantId, cancellationToken);

        if (activity == null)
        {
            throw new KeyNotFoundException($"Activity with Id '{request.Id}' was not found.");
        }

        _context.Activities.Remove(activity);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
