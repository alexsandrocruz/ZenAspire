using System.ComponentModel.DataAnnotations;
using CleanAspire.Domain.Common;
using CleanAspire.Domain.Enums;

namespace CleanAspire.Domain.Entities;

/// <summary>
/// Represents a consent record for LGPD compliance.
/// Tracks user consent for different data processing purposes.
/// </summary>
public class Consent : BaseAuditableEntity, IAuditTrial
{
    /// <summary>
    /// Tenant identifier for multi-tenancy support
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Type of the entity that owns this consent
    /// </summary>
    [Required]
    public OwnerType OwnerType { get; set; }

    /// <summary>
    /// Identifier of the entity that owns this consent
    /// </summary>
    [Required]
    public Guid OwnerId { get; set; }

    /// <summary>
    /// Purpose for which consent is given
    /// </summary>
    [Required]
    public ConsentPurpose Purpose { get; set; }

    /// <summary>
    /// Whether consent is given (opt-in) or withdrawn (opt-out)
    /// </summary>
    [Required]
    public bool OptIn { get; set; }

    /// <summary>
    /// Channel through which consent was collected
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Channel { get; set; } = string.Empty;

    /// <summary>
    /// Date and time when consent was recorded
    /// </summary>
    [Required]
    public DateTime At { get; set; }

    /// <summary>
    /// Source or origin of consent (e.g., form name, website, application)
    /// </summary>
    [MaxLength(200)]
    public string? Source { get; set; }

    /// <summary>
    /// IP address from which consent was given
    /// </summary>
    [MaxLength(45)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent of the browser/device used to give consent
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Additional notes or context about the consent
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Validity period of consent (null means indefinite)
    /// </summary>
    public DateTime? ValidUntil { get; set; }

    /// <summary>
    /// Whether this consent record is still active
    /// </summary>
    public bool IsActive => OptIn && (ValidUntil == null || ValidUntil > DateTime.UtcNow);

    // Navigation properties
    public virtual Client? Client { get; set; }
    public virtual Contact? Contact { get; set; }
}