namespace CleanAspire.ClientApp.DTOs;

/// <summary>
/// Temporary Contact DTO for frontend use.
/// This will be replaced by API client generated models.
/// </summary>
public class ContactDto
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? MobilePhone { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Notes { get; set; }
    public string Tags { get; set; } = string.Empty;
    public int Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public bool IsMainContact { get; set; }
    public bool IsDecisionMaker { get; set; }
    public DateTime? BirthDate { get; set; }
    public DateTime? LastContactDate { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string? ClientName { get; set; }
    public string? ClientDisplayName { get; set; }
    public DateTime? Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
    public string CompanyName => ClientDisplayName ?? ClientName ?? string.Empty;
}