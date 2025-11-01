namespace CleanAspire.ClientApp.DTOs;

/// <summary>
/// Tag DTO for frontend use
/// </summary>
public class TagDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public string? Description { get; set; }
    public DateTime Created { get; set; }
}

/// <summary>
/// Request DTO for linking/unlinking tags
/// </summary>
public class TagLinkRequest
{
    public Guid TagId { get; set; }
    public OwnerType OwnerType { get; set; }
    public Guid OwnerId { get; set; }
}

/// <summary>
/// Request DTO for creating a new tag
/// </summary>
public class TagCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public string? Description { get; set; }
}
