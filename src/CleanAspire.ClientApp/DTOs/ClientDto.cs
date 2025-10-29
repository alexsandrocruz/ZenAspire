namespace CleanAspire.ClientApp.DTOs;

/// <summary>
/// Temporary Client DTO for frontend use.
/// This will be replaced by API client generated models.
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
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? Industry { get; set; }
    public string? Size { get; set; }
    public decimal? AnnualRevenue { get; set; }
    public int? EmployeeCount { get; set; }
    public int Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string PriorityName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? Tags { get; set; }
    public DateTime? FirstContactDate { get; set; }
    public DateTime? LastInteractionDate { get; set; }
    public DateTime? Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
    public int ContactsCount { get; set; }

    public string DisplayName => !string.IsNullOrEmpty(TradeName) ? TradeName : Name;
}