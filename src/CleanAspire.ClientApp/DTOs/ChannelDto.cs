namespace CleanAspire.ClientApp.DTOs;

/// <summary>
/// Communication channel DTO for frontend use
/// </summary>
public class ChannelDto
{
    public Guid Id { get; set; }
    public OwnerType OwnerType { get; set; }
    public Guid OwnerId { get; set; }
    public ChannelType Type { get; set; }
    public string Value { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? Label { get; set; }
    public bool OptedIn { get; set; }
    public DateTime? OptedInAt { get; set; }
    public string DisplayValue { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

/// <summary>
/// Channel type enum (matching Domain)
/// </summary>
public enum ChannelType
{
    Email = 1,
    Phone = 2,
    Mobile = 3,
    WhatsApp = 4,
    Website = 5,
    Instagram = 6,
    LinkedIn = 7,
    Facebook = 8,
    Twitter = 9,
    Telegram = 10,
    Skype = 11,
    Other = 99
}

/// <summary>
/// Request DTO for creating/updating channels
/// </summary>
public class ChannelRequest
{
    public OwnerType OwnerType { get; set; }
    public Guid OwnerId { get; set; }
    public ChannelType Type { get; set; }
    public string Value { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public string? Label { get; set; }
    public bool OptedIn { get; set; } = true;
}
