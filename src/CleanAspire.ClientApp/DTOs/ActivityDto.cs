namespace CleanAspire.ClientApp.DTOs;

/// <summary>
/// Activity DTO for frontend use
/// </summary>
public class ActivityDto
{
    public Guid Id { get; set; }
    public ActivityType Type { get; set; }
    public ActivityStatus Status { get; set; }
    public DateTime Start { get; set; }
    public DateTime? Due { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public RegardingType? RegardingType { get; set; }
    public Guid? RegardingId { get; set; }
    public string? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public DateTime? ReminderAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public ActivityPriority Priority { get; set; }
    public string? Location { get; set; }
    public int? DurationMinutes { get; set; }
    public bool IsOverdue { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
}

/// <summary>
/// Activity type enum (matching Domain)
/// </summary>
public enum ActivityType
{
    Task = 1,
    PhoneCall = 2,
    Meeting = 3,
    FollowUp = 4,
    Email = 10,
    WhatsAppMessage = 11,
    WhatsAppFlow = 12,
    SMS = 13,
    InstagramDM = 20,
    FacebookMessage = 21,
    LinkedInMessage = 22,
    TwitterDM = 23,
    VideoCall = 30,
    WebChat = 31,
    Chatbot = 32,
    Event = 40,
    Webinar = 41,
    Presentation = 42,
    Workshop = 43,
    Newsletter = 50,
    Campaign = 51,
    Survey = 52,
    Note = 60,
    Document = 61,
    Other = 99
}

/// <summary>
/// Activity status enum (matching Domain)
/// </summary>
public enum ActivityStatus
{
    Open = 1,
    InProgress = 2,
    Completed = 3,
    Canceled = 4,
    Deferred = 5
}

/// <summary>
/// Activity priority enum (matching Domain)
/// </summary>
public enum ActivityPriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Urgent = 4
}

/// <summary>
/// Regarding type enum (matching Domain)
/// </summary>
public enum RegardingType
{
    Client = 1,
    Contact = 2,
    Opportunity = 3,
    Case = 4,
    Lead = 5,
    Campaign = 6,
    Project = 7
}

/// <summary>
/// Request DTO for creating/updating activities
/// </summary>
public class ActivityRequest
{
    public ActivityType Type { get; set; }
    public ActivityStatus Status { get; set; }
    public DateTime Start { get; set; }
    public DateTime? Due { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public RegardingType? RegardingType { get; set; }
    public Guid? RegardingId { get; set; }
    public string? AssignedToUserId { get; set; }
    public DateTime? ReminderAt { get; set; }
    public ActivityPriority Priority { get; set; }
    public string? Location { get; set; }
    public int? DurationMinutes { get; set; }
}
