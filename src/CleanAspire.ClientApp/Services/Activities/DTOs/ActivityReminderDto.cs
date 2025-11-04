namespace CleanAspire.ClientApp.Services.Activities.DTOs;

/// <summary>
/// DTO for activity reminder information
/// Part of Phase 3: Timeline - Basic Reminder System
/// </summary>
public class ActivityReminderDto
{
    /// <summary>
    /// Activity ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Activity subject/title
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Activity description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Activity type
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    /// Activity priority
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// When the reminder should be triggered
    /// </summary>
    public DateTime ReminderAt { get; set; }

    /// <summary>
    /// When the activity is scheduled to start
    /// </summary>
    public DateTime Start { get; set; }

    /// <summary>
    /// Due date of the activity
    /// </summary>
    public DateTime? Due { get; set; }

    /// <summary>
    /// Type of entity this activity is regarding
    /// </summary>
    public int? RegardingType { get; set; }

    /// <summary>
    /// ID of the entity this activity is regarding
    /// </summary>
    public Guid? RegardingId { get; set; }

    /// <summary>
    /// Name of the related entity (if available)
    /// </summary>
    public string? RegardingName { get; set; }

    /// <summary>
    /// User ID assigned to the activity
    /// </summary>
    public string? AssignedToUserId { get; set; }

    /// <summary>
    /// Location for the activity
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Whether the reminder has been sent
    /// </summary>
    public bool ReminderSent { get; set; }

    /// <summary>
    /// Time until reminder (for display purposes)
    /// </summary>
    public string TimeUntilReminder { get; set; } = string.Empty;
}