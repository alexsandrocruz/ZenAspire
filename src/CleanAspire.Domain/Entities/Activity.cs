using System.ComponentModel.DataAnnotations;
using CleanAspire.Domain.Common;

namespace CleanAspire.Domain.Entities;

/// <summary>
/// Represents a scheduled or completed activity in the CRM.
/// Activities are planned actions that need to be completed.
/// Part of Phase 3: Timeline - Activities & Interactions
/// </summary>
public class Activity : BaseAuditableEntity
{
    /// <summary>
    /// Tenant identifier for multi-tenancy support
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Type of activity (Task, PhoneCall, Email, WhatsApp, etc.)
    /// </summary>
    [Required]
    public ActivityType Type { get; set; }

    /// <summary>
    /// Current status of the activity (Open, Completed, Canceled)
    /// </summary>
    [Required]
    public ActivityStatus Status { get; set; } = ActivityStatus.Open;

    /// <summary>
    /// Activity start date/time
    /// </summary>
    [Required]
    public DateTime Start { get; set; }

    /// <summary>
    /// Due date/time for the activity (nullable for open-ended activities)
    /// </summary>
    public DateTime? Due { get; set; }

    /// <summary>
    /// Brief subject/title of the activity (max 160 chars)
    /// </summary>
    [Required]
    [MaxLength(160)]
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the activity
    /// </summary>
    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>
    /// Type of entity this activity is regarding (Client, Contact, Opportunity, Case, etc.)
    /// </summary>
    public RegardingType? RegardingType { get; set; }

    /// <summary>
    /// ID of the entity this activity is regarding
    /// </summary>
    public Guid? RegardingId { get; set; }

    /// <summary>
    /// User ID of the person assigned to this activity
    /// </summary>
    [MaxLength(450)]
    public string? AssignedToUserId { get; set; }

    /// <summary>
    /// When to send a reminder notification (optional)
    /// </summary>
    public DateTime? ReminderAt { get; set; }

    /// <summary>
    /// Whether the reminder has been sent
    /// </summary>
    public bool ReminderSent { get; set; }

    /// <summary>
    /// When the activity was actually completed (null if not completed)
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Priority level of the activity
    /// </summary>
    public ActivityPriority Priority { get; set; } = ActivityPriority.Normal;

    /// <summary>
    /// Location for the activity (for meetings, events, etc.)
    /// </summary>
    [MaxLength(200)]
    public string? Location { get; set; }

    /// <summary>
    /// Duration in minutes (for meetings, calls, etc.)
    /// </summary>
    public int? DurationMinutes { get; set; }

    // Computed properties
    public bool IsOverdue => Status == ActivityStatus.Open && Due.HasValue && Due.Value < DateTime.UtcNow;
    public bool IsCompleted => Status == ActivityStatus.Completed;
    public bool IsCanceled => Status == ActivityStatus.Canceled;
}

/// <summary>
/// Types of activities that can be tracked in the CRM - covers modern omnichannel interactions
/// </summary>
public enum ActivityType
{
    // ✅ Traditional activities
    Task = 1,           // Generic task or to-do item
    PhoneCall = 2,      // Phone call to make
    Meeting = 3,        // In-person or virtual meeting
    FollowUp = 4,       // Follow-up action

    // ✅ Email & Messaging
    Email = 10,         // Send email
    WhatsAppMessage = 11, // Send WhatsApp message
    WhatsAppFlow = 12,  // WhatsApp Flow interaction (forms, buttons, etc.)
    SMS = 13,           // Send SMS

    // ✅ Social Media
    InstagramDM = 20,   // Send Instagram Direct Message
    FacebookMessage = 21, // Send Facebook Messenger message
    LinkedInMessage = 22, // Send LinkedIn message
    TwitterDM = 23,     // Send Twitter/X Direct Message

    // ✅ Modern Communication
    VideoCall = 30,     // Schedule video call (Zoom, Meet, Teams, etc.)
    WebChat = 31,       // Live chat interaction
    Chatbot = 32,       // Chatbot follow-up

    // ✅ Events & Presentations
    Event = 40,         // Event attendance or organization
    Webinar = 41,       // Webinar presentation
    Presentation = 42,  // In-person presentation
    Workshop = 43,      // Workshop or training session

    // ✅ Content & Marketing
    Newsletter = 50,    // Send newsletter
    Campaign = 51,      // Marketing campaign action
    Survey = 52,        // Send survey

    // ✅ Other
    Note = 60,          // Add note/observation
    Document = 61,      // Review or send document
    Other = 99          // Other activity type
}

/// <summary>
/// Activity status workflow
/// </summary>
public enum ActivityStatus
{
    Open = 1,       // Activity is pending
    InProgress = 2, // Activity is being worked on
    Completed = 3,  // Activity has been completed
    Canceled = 4,   // Activity was canceled
    Deferred = 5    // Activity was postponed
}

/// <summary>
/// Activity priority levels
/// </summary>
public enum ActivityPriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Urgent = 4
}

/// <summary>
/// Types of entities an activity can be related to
/// </summary>
public enum RegardingType
{
    Client = 1,
    Contact = 2,
    Opportunity = 3,  // For future sales pipeline features
    Case = 4,         // For support/service cases
    Lead = 5,         // For lead management
    Campaign = 6,     // For marketing campaigns
    Project = 7       // For project management
}
