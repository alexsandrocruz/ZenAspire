// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace CleanAspire.Application.Features.Timeline.DTOs;

/// <summary>
/// Unified timeline item that can represent Activity, Interaction, or Note.
/// Provides a consistent view of all timeline events.
/// </summary>
public class TimelineItemDto
{
    /// <summary>
    /// Unique identifier for the item
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Type of timeline item (Activity, Interaction, Note)
    /// </summary>
    public TimelineItemType Type { get; set; }

    /// <summary>
    /// Date/time of the timeline item
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Title or subject of the timeline item
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Description or snippet of the timeline item
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Icon identifier for UI rendering (e.g., "email", "phone", "note")
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Badge text (e.g., "Completed", "Inbound", "Pinned")
    /// </summary>
    public string? Badge { get; set; }

    /// <summary>
    /// Color/style hint for UI rendering (e.g., "success", "info", "warning")
    /// </summary>
    public string? ColorHint { get; set; }

    /// <summary>
    /// Type-specific code (ActivityType, InteractionType, or "Note")
    /// </summary>
    public string TypeCode { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable type name
    /// </summary>
    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// Status for activities ("Open", "Completed", etc.) or direction for interactions ("Inbound", "Outbound")
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Duration in seconds (for interactions) or minutes (for activities)
    /// </summary>
    public int? Duration { get; set; }

    /// <summary>
    /// User who created or handled the item
    /// </summary>
    public string? User { get; set; }

    /// <summary>
    /// Additional metadata as JSON (optional)
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Whether this item is pinned (for notes)
    /// </summary>
    public bool IsPinned { get; set; }

    /// <summary>
    /// Whether this item is private (for notes)
    /// </summary>
    public bool IsPrivate { get; set; }

    /// <summary>
    /// When the item was created in the system
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// User who created the item in the system
    /// </summary>
    public string? CreatedBy { get; set; }
}

/// <summary>
/// Type of timeline item
/// </summary>
public enum TimelineItemType
{
    /// <summary>
    /// Scheduled or completed activity
    /// </summary>
    Activity = 1,

    /// <summary>
    /// Completed interaction (past event)
    /// </summary>
    Interaction = 2,

    /// <summary>
    /// Note or comment
    /// </summary>
    Note = 3
}
