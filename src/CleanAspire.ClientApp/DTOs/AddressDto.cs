namespace CleanAspire.ClientApp.DTOs;

/// <summary>
/// Address DTO for frontend use
/// </summary>
public class AddressDto
{
    public Guid Id { get; set; }
    public OwnerType OwnerType { get; set; }
    public Guid OwnerId { get; set; }
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string? District { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Zip { get; set; } = string.Empty;
    public string Country { get; set; } = "BR";
    public decimal? GeoLat { get; set; }
    public decimal? GeoLng { get; set; }
    public bool IsPrimary { get; set; }
    public string? Label { get; set; }
    public string FullAddress { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

/// <summary>
/// Request DTO for creating/updating addresses
/// </summary>
public class AddressRequest
{
    public OwnerType OwnerType { get; set; }
    public Guid OwnerId { get; set; }
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string? District { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Zip { get; set; } = string.Empty;
    public string Country { get; set; } = "BR";
    public decimal? GeoLat { get; set; }
    public decimal? GeoLng { get; set; }
    public bool IsPrimary { get; set; }
    public string? Label { get; set; }
}
