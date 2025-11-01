// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Domain.Entities;

namespace CleanAspire.Application.Features.Activities.DTOs;

/// <summary>
/// Data Transfer Object for Activity entity.
/// Contains full activity information including all properties and computed fields.
/// </summary>
public class ActivityDto
{
    /// <summary>
    /// Unique identifier for the activity
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Type of activity (Task, PhoneCall, Email, WhatsApp, etc.)
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    /// Human-readable name of the activity type
    /// </summary>
    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the activity (Open, InProgress, Completed, Canceled, Deferred)
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Human-readable name of the activity status
    /// </summary>
    public string StatusName { get; set; } = string.Empty;

    /// <summary>
    /// Activity start date/time
    /// </summary>
    public DateTime Start { get; set; }

    /// <summary>
    /// Due date/time for the activity (nullable for open-ended activities)
    /// </summary>
    public DateTime? Due { get; set; }

    /// <summary>
    /// Brief subject/title of the activity
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the activity
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Type of entity this activity is regarding (Client, Contact, Opportunity, etc.)
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
    /// When to send a reminder notification
    /// </summary>
    public DateTime? ReminderAt { get; set; }

    /// <summary>
    /// When the activity was actually completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Priority level of the activity (Low, Normal, High, Urgent)
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Human-readable name of the priority level
    /// </summary>
    public string PriorityName { get; set; } = string.Empty;

    /// <summary>
    /// Location for the activity (for meetings, events, etc.)
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Duration in minutes (for meetings, calls, etc.)
    /// </summary>
    public int? DurationMinutes { get; set; }

    // Audit Information
    /// <summary>
    /// Date and time when the activity was created
    /// </summary>
    public DateTime? Created { get; set; }

    /// <summary>
    /// User who created the activity
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Date and time when the activity was last modified
    /// </summary>
    public DateTime? LastModified { get; set; }

    /// <summary>
    /// User who last modified the activity
    /// </summary>
    public string? LastModifiedBy { get; set; }

    // Computed Properties
    /// <summary>
    /// Indicates whether the activity is overdue
    /// </summary>
    public bool IsOverdue { get; set; }

    /// <summary>
    /// Indicates whether the activity is completed
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Indicates whether the activity is canceled
    /// </summary>
    public bool IsCanceled { get; set; }
}
