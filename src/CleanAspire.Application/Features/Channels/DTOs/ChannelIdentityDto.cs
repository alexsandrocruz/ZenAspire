// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Domain.Enums;

namespace CleanAspire.Application.Features.Channels.DTOs;

/// <summary>
/// Data Transfer Object for ChannelIdentity entity
/// </summary>
public class ChannelIdentityDto
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public OwnerType OwnerType { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public ChannelType Type { get; set; }
    public string Value { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public bool IsVerified { get; set; }
    public string? Label { get; set; }
    public bool OptedIn { get; set; }
    public DateTime? OptedInAt { get; set; }
    public string DisplayValue { get; set; } = string.Empty;
    public DateTime? Created { get; set; }
    public DateTime? LastModified { get; set; }
}
