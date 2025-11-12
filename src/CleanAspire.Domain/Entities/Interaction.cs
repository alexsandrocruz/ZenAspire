using System.ComponentModel.DataAnnotations;
using CleanAspire.Domain.Common;
using CleanAspire.Domain.Enums;

namespace CleanAspire.Domain.Entities;

/// <summary>
/// Represents a completed interaction (past event) in the CRM timeline.
/// Interactions capture the history of communications and touchpoints with clients/contacts.
/// Part of Phase 3: Timeline - Activities & Interactions
/// </summary>
public class Interaction : BaseAuditableEntity
{
    /// <summary>
    /// Tenant identifier for multi-tenancy support
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Type of interaction (Email, WhatsApp, Phone, etc.)
    /// </summary>
    [Required]
    public InteractionType Type { get; set; }

    /// <summary>
    /// Direction of the interaction (Inbound, Outbound)
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Direction { get; set; } = InteractionDirection.Outbound;

    /// <summary>
    /// When the interaction occurred
    /// </summary>
    [Required]
    public DateTime At { get; set; }

    /// <summary>
    /// Type of entity this interaction is related to (Client, Contact, etc.)
    /// </summary>
    [Required]
    public OwnerType OwnerType { get; set; }

    /// <summary>
    /// ID of the entity this interaction is related to
    /// </summary>
    [Required]
    public Guid OwnerId { get; set; }

    /// <summary>
    /// External channel reference (message ID, email ID, call ID, etc.)
    /// Useful for deduplication and linking to external systems
    /// </summary>
    [MaxLength(256)]
    public string? ChannelRef { get; set; }

    /// <summary>
    /// Subject or title of the interaction (max 200 chars)
    /// </summary>
    [MaxLength(200)]
    public string? Subject { get; set; }

    /// <summary>
    /// Brief snippet or summary of the interaction content (max 500 chars)
    /// </summary>
    [MaxLength(500)]
    public string? Snippet { get; set; }

    /// <summary>
    /// Full payload as JSON (flexible storage for channel-specific data)
    /// Examples:
    /// - Email: { "from", "to", "cc", "body", "attachments" }
    /// - WhatsApp: { "phone", "message", "mediaUrl", "flowId" }
    /// - Phone: { "phone", "duration", "recording", "outcome" }
    /// </summary>
    public string? PayloadJson { get; set; }

    /// <summary>
    /// Duration in seconds (for calls, meetings, etc.)
    /// </summary>
    public int? DurationSeconds { get; set; }

    /// <summary>
    /// User ID of the internal user who handled this interaction
    /// </summary>
    [MaxLength(450)]
    public string? HandledByUserId { get; set; }

    /// <summary>
    /// Sentiment analysis result (Positive, Neutral, Negative) - optional AI feature
    /// </summary>
    [MaxLength(20)]
    public string? Sentiment { get; set; }

    /// <summary>
    /// Tags for categorizing interactions (comma-separated for quick filtering)
    /// </summary>
    [MaxLength(500)]
    public string? Tags { get; set; }

    // Computed properties
    public bool IsInbound => Direction == InteractionDirection.Inbound;
    public bool IsOutbound => Direction == InteractionDirection.Outbound;
}

/// <summary>
/// Types of interactions that can be captured in the CRM - covers all modern communication channels
/// </summary>
public enum InteractionType
{
    // ✅ Email
    Email = 1,              // Email sent or received

    // ✅ WhatsApp (Meta Business Platform)
    WhatsApp = 10,          // WhatsApp message
    WhatsAppFlow = 11,      // WhatsApp Flow interaction (forms, CTAs)
    WhatsAppVoice = 12,     // WhatsApp voice message
    WhatsAppVideo = 13,     // WhatsApp video message

    // ✅ SMS & Messaging
    SMS = 20,               // SMS message
    RCS = 21,               // Rich Communication Services (Google RCS)

    // ✅ Phone
    PhoneCall = 30,         // Phone call (inbound or outbound)
    Voicemail = 31,         // Voicemail message

    // ✅ Social Media - Instagram
    InstagramDM = 40,       // Instagram Direct Message
    InstagramComment = 41,  // Comment on Instagram post
    InstagramMention = 42,  // Mention in Instagram story/post

    // ✅ Social Media - Facebook
    FacebookMessage = 50,   // Facebook Messenger message
    FacebookComment = 51,   // Comment on Facebook post
    FacebookMention = 52,   // Tag/mention on Facebook

    // ✅ Social Media - LinkedIn
    LinkedInMessage = 60,   // LinkedIn message
    LinkedInComment = 61,   // Comment on LinkedIn post
    LinkedInConnection = 62, // Connection request/acceptance

    // ✅ Social Media - Twitter/X
    TwitterDM = 70,         // Twitter/X Direct Message
    TwitterMention = 71,    // Mention or reply on Twitter/X
    TwitterRetweet = 72,    // Retweet with comment

    // ✅ Social Media - TikTok
    TikTokDM = 80,          // TikTok Direct Message
    TikTokComment = 81,     // Comment on TikTok video

    // ✅ Web & Chat
    WebChat = 90,           // Live chat on website
    Chatbot = 91,           // Chatbot conversation
    WebForm = 92,           // Web form submission
    ContactForm = 93,       // Contact form submission

    // ✅ Video Calls
    VideoCall = 100,        // Video call (Zoom, Meet, Teams, etc.)
    ZoomMeeting = 101,      // Zoom meeting
    GoogleMeet = 102,       // Google Meet
    MicrosoftTeams = 103,   // Microsoft Teams meeting

    // ✅ Events & In-Person
    Meeting = 110,          // In-person meeting
    Event = 111,            // Event attendance
    TradeShow = 112,        // Trade show interaction
    Conference = 113,       // Conference attendance

    // ✅ Marketing & Campaigns
    NewsletterOpen = 120,   // Newsletter opened
    NewsletterClick = 121,  // Newsletter link clicked
    CampaignResponse = 122, // Marketing campaign response
    WebinarAttendance = 123, // Webinar attendance
    SurveyResponse = 124,   // Survey completed

    // ✅ E-commerce & Transactions
    PurchaseCompleted = 130, // Purchase made
    QuoteRequested = 131,   // Quote requested
    ProductDemo = 132,      // Product demo attended

    // ✅ Support & Service
    SupportTicket = 140,    // Support ticket created
    TicketResolved = 141,   // Support ticket resolved
    FeedbackSubmitted = 142, // Feedback form submitted
    ComplaintFiled = 143,   // Complaint filed

    // ✅ System & Data
    Import = 150,           // Data import
    ManualEntry = 151,      // Manual entry by user
    APIWebhook = 152,       // Webhook from external system
    Integration = 153,      // Integration sync (CRM, ERP, etc.)

    // ✅ Other
    Note = 160,             // Manual note added
    Other = 199             // Other interaction type
}

/// <summary>
/// Direction of the interaction
/// </summary>
public static class InteractionDirection
{
    public const string Inbound = "Inbound";   // From client/contact to company
    public const string Outbound = "Outbound"; // From company to client/contact
    public const string Internal = "Internal"; // Internal notes/observations
}

/// <summary>
/// Sentiment analysis results (optional AI feature)
/// </summary>
public static class InteractionSentiment
{
    public const string Positive = "Positive";
    public const string Neutral = "Neutral";
    public const string Negative = "Negative";
    public const string Mixed = "Mixed";
}
