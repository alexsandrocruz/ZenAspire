// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Enums;

namespace CleanAspire.Application.Features.Interactions.DTOs;

/// <summary>
/// Data Transfer Object for Interaction entity.
/// Contains full interaction information including all properties.
/// </summary>
public class InteractionDto
{
    /// <summary>
    /// Unique identifier for the interaction
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Type of interaction (Email, WhatsApp, Phone, etc.)
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    /// Human-readable name of the interaction type
    /// </summary>
    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// Direction of the interaction (Inbound, Outbound, Internal)
    /// </summary>
    public string Direction { get; set; } = string.Empty;

    /// <summary>
    /// When the interaction occurred
    /// </summary>
    public DateTime At { get; set; }

    /// <summary>
    /// Type of entity this interaction is related to (Client, Contact, etc.)
    /// </summary>
    public int OwnerType { get; set; }

    /// <summary>
    /// Human-readable name of the owner entity type
    /// </summary>
    public string OwnerTypeName { get; set; } = string.Empty;

    /// <summary>
    /// ID of the entity this interaction is related to
    /// </summary>
    public string OwnerId { get; set; } = string.Empty;

    /// <summary>
    /// External channel reference (message ID, email ID, call ID, etc.)
    /// </summary>
    public string? ChannelRef { get; set; }

    /// <summary>
    /// Subject or title of the interaction
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Brief snippet or summary of the interaction content
    /// </summary>
    public string? Snippet { get; set; }

    /// <summary>
    /// Full payload as JSON (flexible storage for channel-specific data)
    /// </summary>
    public string? PayloadJson { get; set; }

    /// <summary>
    /// Duration in seconds (for calls, meetings, etc.)
    /// </summary>
    public int? DurationSeconds { get; set; }

    /// <summary>
    /// User ID of the internal user who handled this interaction
    /// </summary>
    public string? HandledByUserId { get; set; }

    /// <summary>
    /// Sentiment analysis result (Positive, Neutral, Negative)
    /// </summary>
    public string? Sentiment { get; set; }

    /// <summary>
    /// Tags for categorizing interactions
    /// </summary>
    public string? Tags { get; set; }

    // Audit Information
    /// <summary>
    /// Date and time when the interaction was created
    /// </summary>
    public DateTime? Created { get; set; }

    /// <summary>
    /// User who created the interaction
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Date and time when the interaction was last modified
    /// </summary>
    public DateTime? LastModified { get; set; }

    /// <summary>
    /// User who last modified the interaction
    /// </summary>
    public string? LastModifiedBy { get; set; }

    // Computed Properties
    /// <summary>
    /// Indicates whether the interaction is inbound
    /// </summary>
    public bool IsInbound { get; set; }

    /// <summary>
    /// Indicates whether the interaction is outbound
    /// </summary>
    public bool IsOutbound { get; set; }
}
