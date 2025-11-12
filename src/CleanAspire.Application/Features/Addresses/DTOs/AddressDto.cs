// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Domain.Enums;

namespace CleanAspire.Application.Features.Addresses.DTOs;

/// <summary>
/// Data Transfer Object for Address entity
/// </summary>
public class AddressDto
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public OwnerType OwnerType { get; set; }
    public string OwnerId { get; set; } = string.Empty;
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
    public DateTime? Created { get; set; }
    public DateTime? LastModified { get; set; }
}
