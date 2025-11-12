// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel.DataAnnotations;
using CleanAspire.Domain.Common;
using CleanAspire.Domain.Enums;

namespace CleanAspire.Domain.Entities;

/// <summary>
/// Represents a normalized communication channel identity.
/// Supports multiple channels per entity (e.g., multiple emails, phones, social media accounts).
/// Enables omni-channel communication tracking.
/// </summary>
public class ChannelIdentity : BaseAuditableEntity, IAuditTrial
{
    /// <summary>
    /// Tenant identifier for multi-tenancy isolation
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Type of entity that owns this channel (Client, Contact, etc.)
    /// </summary>
    [Required]
    public OwnerType OwnerType { get; set; }

    /// <summary>
    /// ID of the entity that owns this channel (polymorphic)
    /// </summary>
    [Required]
    public string OwnerId { get; set; } = string.Empty;

    /// <summary>
    /// Type of communication channel (Email, Phone, WhatsApp, etc.)
    /// </summary>
    [Required]
    public ChannelType Type { get; set; }

    /// <summary>
    /// The actual channel value (email address, phone number, URL, handle, etc.)
    /// Format depends on Type:
    /// - Email: email@example.com
    /// - Phone/Mobile: E.164 format preferred (+55 11 99999-9999)
    /// - WhatsApp: E.164 format preferred
    /// - Website: full URL (https://example.com)
    /// - Instagram/Twitter: @handle or full URL
    /// - LinkedIn: profile URL
    /// </summary>
    [Required]
    [MaxLength(256)]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if this is the primary/default channel of this type for the entity
    /// </summary>
    public bool IsPrimary { get; set; } = false;

    /// <summary>
    /// When this channel was verified (e.g., via OTP, email confirmation)
    /// NULL means not verified
    /// </summary>
    public DateTime? VerifiedAt { get; set; }

    /// <summary>
    /// Optional label for the channel (e.g., "Work", "Personal", "Support")
    /// </summary>
    [MaxLength(50)]
    public string? Label { get; set; }

    /// <summary>
    /// Opt-in status for this channel (for marketing/communication consent)
    /// </summary>
    public bool OptedIn { get; set; } = true;

    /// <summary>
    /// When the opt-in status was last changed
    /// </summary>
    public DateTime? OptedInAt { get; set; }

    /// <summary>
    /// Returns a display-friendly format of the channel
    /// </summary>
    public string DisplayValue
    {
        get
        {
            return Type switch
            {
                ChannelType.Email => Value,
                ChannelType.Phone or ChannelType.Mobile or ChannelType.WhatsApp => FormatPhone(Value),
                ChannelType.Website => Value,
                ChannelType.Instagram or ChannelType.Twitter => Value.StartsWith("@") ? Value : $"@{Value}",
                ChannelType.LinkedIn or ChannelType.Facebook => Value,
                ChannelType.Telegram => Value.StartsWith("@") ? Value : $"@{Value}",
                ChannelType.Skype => Value,
                _ => Value
            };
        }
    }

    /// <summary>
    /// Returns whether this channel is verified
    /// </summary>
    public bool IsVerified => VerifiedAt.HasValue;

    private static string FormatPhone(string phone)
    {
        // Simple formatting - you can enhance this for different countries
        if (string.IsNullOrEmpty(phone)) return phone;

        // Remove non-numeric characters for display
        var digits = new string(phone.Where(char.IsDigit).ToArray());

        // Brazilian format example: +55 11 99999-9999
        if (digits.Length == 13 && digits.StartsWith("55"))
        {
            return $"+{digits[..2]} {digits[2..4]} {digits[4..9]}-{digits[9..]}";
        }

        return phone; // Return as-is if format not recognized
    }
}
