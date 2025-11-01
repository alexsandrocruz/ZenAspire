// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace CleanAspire.Domain.Enums;

/// <summary>
/// Defines types of communication channels for multi-channel support.
/// Used for ChannelIdentity entity.
/// </summary>
public enum ChannelType
{
    /// <summary>
    /// Email address
    /// </summary>
    Email = 1,

    /// <summary>
    /// Landline phone number
    /// </summary>
    Phone = 2,

    /// <summary>
    /// Mobile/cell phone number
    /// </summary>
    Mobile = 3,

    /// <summary>
    /// WhatsApp number (may include country code)
    /// </summary>
    WhatsApp = 4,

    /// <summary>
    /// Website URL
    /// </summary>
    Website = 5,

    /// <summary>
    /// Instagram handle or URL
    /// </summary>
    Instagram = 6,

    /// <summary>
    /// LinkedIn profile URL
    /// </summary>
    LinkedIn = 7,

    /// <summary>
    /// Facebook profile or page URL
    /// </summary>
    Facebook = 8,

    /// <summary>
    /// Twitter/X handle
    /// </summary>
    Twitter = 9,

    /// <summary>
    /// Telegram username or number
    /// </summary>
    Telegram = 10,

    /// <summary>
    /// Skype username
    /// </summary>
    Skype = 11,

    /// <summary>
    /// Other/custom channel type
    /// </summary>
    Other = 99
}
