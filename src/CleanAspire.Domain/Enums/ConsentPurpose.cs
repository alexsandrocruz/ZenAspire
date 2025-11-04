// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace CleanAspire.Domain.Enums;

/// <summary>
/// Defines the purpose of consent for LGPD compliance.
/// Used to track user consent for different data processing activities.
/// </summary>
public enum ConsentPurpose
{
    /// <summary>
    /// Marketing communications and promotional content
    /// </summary>
    Marketing = 1,

    /// <summary>
    /// Contractual fulfillment and service delivery
    /// </summary>
    Contractual = 2,

    /// <summary>
    /// Customer support and service assistance
    /// </summary>
    Support = 3,

    /// <summary>
    /// Legal compliance and regulatory requirements
    /// </summary>
    Legal = 4
}