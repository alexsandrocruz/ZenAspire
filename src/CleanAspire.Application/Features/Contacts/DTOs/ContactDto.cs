namespace CleanAspire.Application.Features.Contacts.DTOs;

/// <summary>
/// Data Transfer Object for Contact entity.
/// Used for API responses and client-server communication.
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
    
    // Personal Address (optional, can be different from Client)
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    
    public string? Notes { get; set; }
    public string? Tags { get; set; }
    
    // Relationship Information
    public int Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    
    public bool IsMainContact { get; set; }
    public bool IsDecisionMaker { get; set; }
    
    // Important Dates
    public DateTime? BirthDate { get; set; }
    public DateTime? LastContactDate { get; set; }
    
    // Client Information
    public string ClientId { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string ClientDisplayName { get; set; } = string.Empty;
    
    // Audit Information
    public DateTime? Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
    
    // Computed Properties
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string ContactInfo => !string.IsNullOrEmpty(Email) ? 
        $"{FullName} ({Email})" : FullName;
    public string RoleInfo => !string.IsNullOrEmpty(JobTitle) ? 
        $"{JobTitle}" + (!string.IsNullOrEmpty(Department) ? $" - {Department}" : "") : 
        Department ?? "";
}