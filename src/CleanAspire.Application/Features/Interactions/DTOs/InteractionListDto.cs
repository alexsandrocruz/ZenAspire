// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace CleanAspire.Application.Features.Interactions.DTOs;

/// <summary>
/// Simplified Data Transfer Object for Interaction entity used in list views.
/// Contains only essential information for displaying interactions in lists.
/// </summary>
public class InteractionListDto
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
    /// Subject or title of the interaction
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Brief snippet or summary of the interaction content
    /// </summary>
    public string? Snippet { get; set; }

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
    /// Date and time when the interaction was created
    /// </summary>
    public DateTime? Created { get; set; }

    /// <summary>
    /// Indicates whether the interaction is inbound
    /// </summary>
    public bool IsInbound { get; set; }

    /// <summary>
    /// Indicates whether the interaction is outbound
    /// </summary>
    public bool IsOutbound { get; set; }
}
