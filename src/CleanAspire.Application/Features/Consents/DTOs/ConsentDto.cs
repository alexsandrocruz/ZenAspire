using CleanAspire.Domain.Enums;

namespace CleanAspire.Application.Features.Consents.DTOs;

/// <summary>
/// Data Transfer Object for Consent entity.
/// Phase 4: LGPD Compliance - Consent & Data Privacy
/// </summary>
public class ConsentDto
{
    public string Id { get; set; } = string.Empty;
    public int OwnerType { get; set; }
    public string OwnerTypeName { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public int Purpose { get; set; }
    public string PurposeName { get; set; } = string.Empty;
    public bool OptIn { get; set; }
    public string Channel { get; set; } = string.Empty;
    public DateTime At { get; set; }
    public string? Source { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Notes { get; set; }
    public DateTime? ValidUntil { get; set; }

    // Audit Information
    public DateTime? Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }

    // Computed Properties
    public bool IsActive { get; set; }
    public bool IsExpired { get; set; }
    public string StatusDescription { get; set; } = string.Empty;
}

/// <summary>
/// DTO for creating a new consent record.
/// </summary>
public class CreateConsentDto
{
    public int OwnerType { get; set; }
    public Guid OwnerId { get; set; }
    public int Purpose { get; set; }
    public bool OptIn { get; set; }
    public string Channel { get; set; } = string.Empty;
    public DateTime? At { get; set; }
    public string? Source { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Notes { get; set; }
    public DateTime? ValidUntil { get; set; }
}

/// <summary>
/// DTO for updating an existing consent record.
/// </summary>
public class UpdateConsentDto
{
    public bool OptIn { get; set; }
    public string? Notes { get; set; }
    public DateTime? ValidUntil { get; set; }
}

/// <summary>
/// DTO for consent list responses with pagination.
/// </summary>
public class ConsentListDto
{
    public string Id { get; set; } = string.Empty;
    public string OwnerTypeName { get; set; } = string.Empty;
    public string OwnerDisplayName { get; set; } = string.Empty;
    public string PurposeName { get; set; } = string.Empty;
    public bool OptIn { get; set; }
    public string Channel { get; set; } = string.Empty;
    public DateTime At { get; set; }
    public DateTime? ValidUntil { get; set; }
    public bool IsActive { get; set; }
    public string StatusDescription { get; set; } = string.Empty;
}

/// <summary>
/// DTO for personal data export requests.
/// Phase 4: LGPD Compliance - Data Export
/// </summary>
public class DataExportRequestDto
{
    public int OwnerType { get; set; }
    public Guid OwnerId { get; set; }
    public string Format { get; set; } = "JSON"; // JSON or CSV
}

/// <summary>
/// DTO for data erasure requests.
/// Phase 4: LGPD Compliance - Data Erasure
/// </summary>
public class DataErasureRequestDto
{
    public int OwnerType { get; set; }
    public Guid OwnerId { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// DTO for export/erasure request responses.
/// </summary>
public class DataProcessingRequestResponseDto
{
    public string RequestId { get; set; } = string.Empty;
    public int OwnerType { get; set; }
    public string OwnerTypeName { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public string RequestType { get; set; } = string.Empty; // "Export" or "Erasure"
    public string Status { get; set; } = string.Empty; // "Pending", "Processing", "Completed", "Failed"
    public DateTime RequestedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? DownloadUrl { get; set; } // For export requests
    public string? ErrorMessage { get; set; } // For failed requests
}