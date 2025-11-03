namespace CleanAspire.ClientApp.DTOs;

/// <summary>
/// Enum matching Domain.Enums.OwnerType
/// </summary>
public enum OwnerType
{
    Client = 1,
    Contact = 2,
    Activity = 3,
    Opportunity = 4,
    Deal = 5,
    Case = 6,
    Lead = 7,
    Campaign = 8,
    Interaction = 9
}

/// <summary>
/// Timeline item DTO matching Application layer
/// </summary>
public class TimelineItemDto
{
    public string Id { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonIgnore]
    public TimelineItemType Type { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("type")]
    public object TypeValue
    {
        get => (int)Type;
        set
        {
            if (value is int intValue)
            {
                Type = (TimelineItemType)intValue;
            }
            else if (value is string stringValue && int.TryParse(stringValue, out int parsedInt))
            {
                Type = (TimelineItemType)parsedInt;
            }
            else if (value is string stringValue2)
            {
                // Try to parse by enum name
                Type = stringValue2 switch
                {
                    "Activity" => TimelineItemType.Activity,
                    "Interaction" => TimelineItemType.Interaction,
                    "Note" => TimelineItemType.Note,
                    _ => TimelineItemType.Activity // Default fallback
                };
            }
            else
            {
                Type = TimelineItemType.Activity; // Default fallback
            }
        }
    }

    public DateTime Date { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string? Badge { get; set; }
    public string? ColorHint { get; set; }
    public string TypeCode { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public string? Status { get; set; }
    public int? Duration { get; set; }
    public string? User { get; set; }
    public string? Metadata { get; set; }
    public bool IsPinned { get; set; }
    public bool IsPrivate { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

/// <summary>
/// Type of timeline item
/// </summary>
public enum TimelineItemType
{
    Activity = 1,
    Interaction = 2,
    Note = 3
}
