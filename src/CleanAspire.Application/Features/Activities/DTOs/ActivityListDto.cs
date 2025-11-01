// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace CleanAspire.Application.Features.Activities.DTOs;

/// <summary>
/// Simplified Data Transfer Object for Activity entity used in list views.
/// Contains only essential fields for better performance in list/grid scenarios.
/// </summary>
public class ActivityListDto
{
    /// <summary>
    /// Unique identifier for the activity
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Type of activity (Task, PhoneCall, Email, etc.)
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    /// Human-readable name of the activity type
    /// </summary>
    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the activity
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Human-readable name of the activity status
    /// </summary>
    public string StatusName { get; set; } = string.Empty;

    /// <summary>
    /// Brief subject/title of the activity
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Activity start date/time
    /// </summary>
    public DateTime Start { get; set; }

    /// <summary>
    /// Due date/time for the activity
    /// </summary>
    public DateTime? Due { get; set; }

    /// <summary>
    /// Priority level of the activity
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Human-readable name of the priority level
    /// </summary>
    public string PriorityName { get; set; } = string.Empty;

    /// <summary>
    /// Type of entity this activity is regarding
    /// </summary>
    public int? RegardingType { get; set; }

    /// <summary>
    /// Human-readable name of the regarding entity type
    /// </summary>
    public string? RegardingTypeName { get; set; }

    /// <summary>
    /// ID of the entity this activity is regarding
    /// </summary>
    public Guid? RegardingId { get; set; }

    /// <summary>
    /// User ID of the person assigned to this activity
    /// </summary>
    public string? AssignedToUserId { get; set; }

    /// <summary>
    /// When the activity was actually completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Indicates whether the activity is overdue
    /// </summary>
    public bool IsOverdue { get; set; }

    /// <summary>
    /// Indicates whether the activity is completed
    /// </summary>
    public bool IsCompleted { get; set; }
}
