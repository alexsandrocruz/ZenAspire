// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Common.Interfaces.FusionCache;
using CleanAspire.Application.Features.Activities.DTOs;
using CleanAspire.Application.Pipeline;
using CleanAspire.Domain.Entities;

namespace CleanAspire.Application.Features.Activities.Commands;

/// <summary>
/// Command for creating a new activity.
/// Encapsulates all data needed to create an activity entity.
/// </summary>
public record CreateActivityCommand : IFusionCacheRefreshRequest<ActivityDto>, IRequiresValidation
{
    /// <summary>
    /// Type of activity (Task, PhoneCall, Email, WhatsApp, etc.)
    /// </summary>
    public ActivityType Type { get; init; } = ActivityType.Task;

    /// <summary>
    /// Current status of the activity (defaults to Open)
    /// </summary>
    public ActivityStatus Status { get; init; } = ActivityStatus.Open;

    /// <summary>
    /// Activity start date/time
    /// </summary>
    public DateTime Start { get; init; }

    /// <summary>
    /// Due date/time for the activity (nullable for open-ended activities)
    /// </summary>
    public DateTime? Due { get; init; }

    /// <summary>
    /// Brief subject/title of the activity (max 160 chars)
    /// </summary>
    public string Subject { get; init; } = string.Empty;

    /// <summary>
    /// Detailed description of the activity
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Type of entity this activity is regarding (Client, Contact, Opportunity, etc.)
    /// </summary>
    public RegardingType? RegardingType { get; init; }

    /// <summary>
    /// ID of the entity this activity is regarding
    /// </summary>
    public Guid? RegardingId { get; init; }

    /// <summary>
    /// User ID of the person assigned to this activity (defaults to current user)
    /// </summary>
    public string? AssignedToUserId { get; init; }

    /// <summary>
    /// When to send a reminder notification (optional)
    /// </summary>
    public DateTime? ReminderAt { get; init; }

    /// <summary>
    /// Whether the reminder has been sent (defaults to false)
    /// </summary>
    public bool ReminderSent { get; init; } = false;

    /// <summary>
    /// Priority level of the activity (defaults to Normal)
    /// </summary>
    public ActivityPriority Priority { get; init; } = ActivityPriority.Normal;

    /// <summary>
    /// Location for the activity (for meetings, events, etc.)
    /// </summary>
    public string? Location { get; init; }

    /// <summary>
    /// Duration in minutes (for meetings, calls, etc.)
    /// </summary>
    public int? DurationMinutes { get; init; }

    /// <summary>
    /// Tags for cache invalidation
    /// </summary>
    public IEnumerable<string>? Tags => new[] { "activities" };
}

/// <summary>
/// Handler for processing CreateActivityCommand.
/// Creates a new activity entity and saves it to the database.
/// </summary>
public class CreateActivityCommandHandler : IRequestHandler<CreateActivityCommand, ActivityDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateActivityCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<ActivityDto> Handle(CreateActivityCommand request, CancellationToken cancellationToken)
    {
        // Validate tenant
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            throw new UnauthorizedAccessException("TenantId is required");

        var activity = new Activity
        {
            TenantId = _currentUser.TenantId,
            Type = request.Type,
            Status = request.Status,
            Start = request.Start,
            Due = request.Due,
            Subject = request.Subject,
            Description = request.Description,
            RegardingType = request.RegardingType,
            RegardingId = request.RegardingId,
            AssignedToUserId = request.AssignedToUserId ?? _currentUser.UserId,
            ReminderAt = request.ReminderAt,
            ReminderSent = request.ReminderSent,
            Priority = request.Priority,
            Location = request.Location,
            DurationMinutes = request.DurationMinutes
        };

        _context.Activities.Add(activity);
        await _context.SaveChangesAsync(cancellationToken);

        return new ActivityDto
        {
            Id = activity.Id,
            Type = (int)activity.Type,
            TypeName = activity.Type.ToString(),
            Status = (int)activity.Status,
            StatusName = activity.Status.ToString(),
            Start = activity.Start,
            Due = activity.Due,
            Subject = activity.Subject,
            Description = activity.Description,
            RegardingType = activity.RegardingType.HasValue ? (int)activity.RegardingType.Value : null,
            RegardingTypeName = activity.RegardingType?.ToString(),
            RegardingId = activity.RegardingId,
            AssignedToUserId = activity.AssignedToUserId,
            ReminderAt = activity.ReminderAt,
            ReminderSent = activity.ReminderSent,
            CompletedAt = activity.CompletedAt,
            Priority = (int)activity.Priority,
            PriorityName = activity.Priority.ToString(),
            Location = activity.Location,
            DurationMinutes = activity.DurationMinutes,
            Created = activity.Created,
            CreatedBy = activity.CreatedBy,
            LastModified = activity.LastModified,
            LastModifiedBy = activity.LastModifiedBy,
            IsOverdue = activity.IsOverdue,
            IsCompleted = activity.IsCompleted,
            IsCanceled = activity.IsCanceled
        };
    }
}
