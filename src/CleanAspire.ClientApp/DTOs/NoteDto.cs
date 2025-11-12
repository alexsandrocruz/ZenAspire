namespace CleanAspire.ClientApp.DTOs;

/// <summary>
/// Note DTO for frontend use
/// </summary>
public class NoteDto
{
    public Guid Id { get; set; }
    public OwnerType OwnerType { get; set; }
    public Guid OwnerId { get; set; }
    public string? Title { get; set; }
    public string Body { get; set; } = string.Empty;
    public bool IsPinned { get; set; }
    public bool IsPrivate { get; set; }
    public DateTime Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
}

/// <summary>
/// Request DTO for creating/updating notes
/// </summary>
public class NoteRequest
{
    public OwnerType OwnerType { get; set; }
    public Guid OwnerId { get; set; }
    public string? Title { get; set; }
    public string Body { get; set; } = string.Empty;
    public bool IsPinned { get; set; }
    public bool IsPrivate { get; set; }
}
