namespace CleanAspire.Application.Features.Clients.DTOs;

/// <summary>
/// Data Transfer Object for Client entity.
/// Used for API responses and client-server communication.
/// </summary>
public class ClientDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? TradeName { get; set; }
    public string? DocumentNumber { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    
    // Address
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    
    // Business Information
    public string? Industry { get; set; }
    public string? Size { get; set; }
    public decimal? AnnualRevenue { get; set; }
    public int? EmployeeCount { get; set; }
    
    // Relationship Information
    public int Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string PriorityName { get; set; } = string.Empty;
    
    public string? Notes { get; set; }
    public string? Tags { get; set; }
    
    // Important Dates
    public DateTime? FirstContactDate { get; set; }
    public DateTime? LastInteractionDate { get; set; }
    
    // Audit Information
    public DateTime? Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
    
    // Related Data
    public int ContactsCount { get; set; }
    
    // Computed Properties
    public string DisplayName => !string.IsNullOrEmpty(TradeName) ? TradeName : Name;
    public bool IsCompany => Type == 1; // ClientType.Company
    public bool IsPublicFigure => Type == 2; // ClientType.PublicFigure
}