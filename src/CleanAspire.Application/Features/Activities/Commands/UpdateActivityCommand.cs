// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Common.Interfaces.FusionCache;
using CleanAspire.Application.Pipeline;
using CleanAspire.Domain.Entities;

namespace CleanAspire.Application.Features.Activities.Commands;

/// <summary>
/// Command for updating an existing activity.
/// Contains all updatable activity properties.
/// </summary>
public record UpdateActivityCommand : IFusionCacheRefreshRequest<Unit>, IRequiresValidation
{
    /// <summary>
    /// Unique identifier of the activity to update
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Type of activity (Task, PhoneCall, Email, WhatsApp, etc.)
    /// </summary>
    public ActivityType Type { get; init; }

    /// <summary>
    /// Current status of the activity
    /// </summary>
    public ActivityStatus Status { get; init; }

    /// <summary>
    /// Activity start date/time
    /// </summary>
    public DateTime Start { get; init; }

    /// <summary>
    /// Due date/time for the activity
    /// </summary>
    public DateTime? Due { get; init; }

    /// <summary>
    /// Brief subject/title of the activity
    /// </summary>
    public string Subject { get; init; } = string.Empty;

    /// <summary>
    /// Detailed description of the activity
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Type of entity this activity is regarding
    /// </summary>
    public RegardingType? RegardingType { get; init; }

    /// <summary>
    /// ID of the entity this activity is regarding
    /// </summary>
    public Guid? RegardingId { get; init; }

    /// <summary>
    /// User ID of the person assigned to this activity
    /// </summary>
    public string? AssignedToUserId { get; init; }

    /// <summary>
    /// When to send a reminder notification
    /// </summary>
    public DateTime? ReminderAt { get; init; }

    /// <summary>
    /// Priority level of the activity
    /// </summary>
    public ActivityPriority Priority { get; init; }

    /// <summary>
    /// Location for the activity
    /// </summary>
    public string? Location { get; init; }

    /// <summary>
    /// Duration in minutes
    /// </summary>
    public int? DurationMinutes { get; init; }

    /// <summary>
    /// Tags for cache invalidation
    /// </summary>
    public IEnumerable<string>? Tags => new[] { "activities" };
}

/// <summary>
/// Handler for processing UpdateActivityCommand.
/// Updates an existing activity entity in the database.
/// </summary>
public class UpdateActivityCommandHandler : IRequestHandler<UpdateActivityCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateActivityCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(UpdateActivityCommand request, CancellationToken cancellationToken)
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

        // Update activity properties
        activity.Type = request.Type;
        activity.Status = request.Status;
        activity.Start = request.Start;
        activity.Due = request.Due;
        activity.Subject = request.Subject;
        activity.Description = request.Description;
        activity.RegardingType = request.RegardingType;
        activity.RegardingId = request.RegardingId;
        activity.AssignedToUserId = request.AssignedToUserId;
        activity.ReminderAt = request.ReminderAt;
        activity.Priority = request.Priority;
        activity.Location = request.Location;
        activity.DurationMinutes = request.DurationMinutes;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
