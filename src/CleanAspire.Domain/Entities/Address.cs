// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel.DataAnnotations;
using CleanAspire.Domain.Common;
using CleanAspire.Domain.Enums;

namespace CleanAspire.Domain.Entities;

/// <summary>
/// Represents a normalized address entity that can be associated with various entities.
/// Supports multiple addresses per entity (e.g., billing address, shipping address).
/// </summary>
public class Address : BaseAuditableEntity, IAuditTrial
{
    /// <summary>
    /// Tenant identifier for multi-tenancy isolation
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Type of entity that owns this address (Client, Contact, etc.)
    /// </summary>
    [Required]
    public OwnerType OwnerType { get; set; }

    /// <summary>
    /// ID of the entity that owns this address (polymorphic)
    /// </summary>
    [Required]
    public string OwnerId { get; set; } = string.Empty;

    /// <summary>
    /// Address line 1 (street, number, building)
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Line1 { get; set; } = string.Empty;

    /// <summary>
    /// Address line 2 (apartment, suite, unit, floor) - optional
    /// </summary>
    [MaxLength(200)]
    public string? Line2 { get; set; }

    /// <summary>
    /// District/Neighborhood (Bairro) - optional
    /// </summary>
    [MaxLength(100)]
    public string? District { get; set; }

    /// <summary>
    /// City name
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// State/Province/Region code (e.g., "SP", "RJ")
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Postal/ZIP code
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Zip { get; set; } = string.Empty;

    /// <summary>
    /// Country code (ISO 3166-1 alpha-2, e.g., "BR", "US")
    /// </summary>
    [Required]
    [MaxLength(2)]
    public string Country { get; set; } = "BR";

    /// <summary>
    /// Latitude for geocoding - optional
    /// </summary>
    public decimal? GeoLat { get; set; }

    /// <summary>
    /// Longitude for geocoding - optional
    /// </summary>
    public decimal? GeoLng { get; set; }

    /// <summary>
    /// Indicates if this is the primary/default address for the entity
    /// </summary>
    public bool IsPrimary { get; set; } = false;

    /// <summary>
    /// Optional label for the address (e.g., "Home", "Office", "Billing", "Shipping")
    /// </summary>
    [MaxLength(50)]
    public string? Label { get; set; }

    /// <summary>
    /// Returns the full formatted address
    /// </summary>
    public string FullAddress
    {
        get
        {
            var parts = new List<string>();

            if (!string.IsNullOrEmpty(Line1)) parts.Add(Line1);
            if (!string.IsNullOrEmpty(Line2)) parts.Add(Line2);
            if (!string.IsNullOrEmpty(District)) parts.Add(District);
            if (!string.IsNullOrEmpty(City)) parts.Add(City);
            if (!string.IsNullOrEmpty(State)) parts.Add(State);
            if (!string.IsNullOrEmpty(Zip)) parts.Add(Zip);
            if (!string.IsNullOrEmpty(Country)) parts.Add(Country);

            return string.Join(", ", parts);
        }
    }
}
