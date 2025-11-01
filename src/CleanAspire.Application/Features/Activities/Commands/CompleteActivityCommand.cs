// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Common.Interfaces.FusionCache;
using CleanAspire.Application.Pipeline;
using CleanAspire.Domain.Entities;

namespace CleanAspire.Application.Features.Activities.Commands;

/// <summary>
/// Command for marking an activity as completed.
/// Sets the activity status to Completed and records the completion timestamp.
/// </summary>
public record CompleteActivityCommand : IFusionCacheRefreshRequest<Unit>, IRequiresValidation
{
    /// <summary>
    /// Unique identifier of the activity to complete
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Optional notes about the completion
    /// </summary>
    public string? CompletionNotes { get; init; }

    /// <summary>
    /// Tags for cache invalidation
    /// </summary>
    public IEnumerable<string>? Tags => new[] { "activities" };
}

/// <summary>
/// Handler for processing CompleteActivityCommand.
/// Marks an activity as completed and updates the completion timestamp.
/// </summary>
public class CompleteActivityCommandHandler : IRequestHandler<CompleteActivityCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CompleteActivityCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(CompleteActivityCommand request, CancellationToken cancellationToken)
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

        // Check if already completed
        if (activity.Status == ActivityStatus.Completed)
        {
            throw new InvalidOperationException("Activity is already completed.");
        }

        // Mark as completed
        activity.Status = ActivityStatus.Completed;
        activity.CompletedAt = DateTime.UtcNow;

        // Optionally append completion notes to description
        if (!string.IsNullOrWhiteSpace(request.CompletionNotes))
        {
            activity.Description = string.IsNullOrEmpty(activity.Description)
                ? $"Completion Notes: {request.CompletionNotes}"
                : $"{activity.Description}\n\nCompletion Notes: {request.CompletionNotes}";
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
