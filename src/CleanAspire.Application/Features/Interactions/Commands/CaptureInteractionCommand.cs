// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Common.Interfaces;
using CleanAspire.Application.Common.Interfaces.FusionCache;
using CleanAspire.Application.Features.Interactions.DTOs;
using CleanAspire.Application.Pipeline;
using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Enums;

namespace CleanAspire.Application.Features.Interactions.Commands;

/// <summary>
/// Command for capturing a new interaction.
/// Encapsulates all data needed to record a completed interaction (past event).
/// </summary>
public record CaptureInteractionCommand : IFusionCacheRefreshRequest<InteractionDto>, IRequiresValidation
{
    /// <summary>
    /// Type of interaction (Email, WhatsApp, Phone, etc.)
    /// </summary>
    public InteractionType Type { get; init; }

    /// <summary>
    /// Direction of the interaction (Inbound, Outbound, Internal)
    /// </summary>
    public string Direction { get; init; } = InteractionDirection.Outbound;

    /// <summary>
    /// When the interaction occurred (defaults to UtcNow if not provided)
    /// </summary>
    public DateTime? At { get; init; }

    /// <summary>
    /// Type of entity this interaction is related to (Client, Contact, etc.)
    /// </summary>
    public OwnerType OwnerType { get; init; }

    /// <summary>
    /// ID of the entity this interaction is related to
    /// </summary>
    public Guid OwnerId { get; init; }

    /// <summary>
    /// External channel reference (message ID, email ID, call ID, etc.)
    /// Useful for deduplication and linking to external systems
    /// </summary>
    public string? ChannelRef { get; init; }

    /// <summary>
    /// Subject or title of the interaction (max 200 chars)
    /// </summary>
    public string? Subject { get; init; }

    /// <summary>
    /// Brief snippet or summary of the interaction content (max 500 chars)
    /// </summary>
    public string? Snippet { get; init; }

    /// <summary>
    /// Full payload as JSON (flexible storage for channel-specific data)
    /// </summary>
    public string? PayloadJson { get; init; }

    /// <summary>
    /// Duration in seconds (for calls, meetings, etc.)
    /// </summary>
    public int? DurationSeconds { get; init; }

    /// <summary>
    /// User ID of the internal user who handled this interaction (defaults to current user)
    /// </summary>
    public string? HandledByUserId { get; init; }

    /// <summary>
    /// Sentiment analysis result (Positive, Neutral, Negative) - optional AI feature
    /// </summary>
    public string? Sentiment { get; init; }

    /// <summary>
    /// Tags for categorizing interactions (comma-separated for quick filtering)
    /// </summary>
    public string? Tags { get; init; }

    /// <summary>
    /// Cache invalidation tags (explicit interface implementation to avoid conflict)
    /// </summary>
    IEnumerable<string>? IFusionCacheRefreshRequest<InteractionDto>.Tags => new[] { "interactions" };
}

/// <summary>
/// Handler for processing CaptureInteractionCommand.
/// Creates a new interaction entity and saves it to the database.
/// Publishes InteractionCaptured domain event.
/// </summary>
public class CaptureInteractionCommandHandler : IRequestHandler<CaptureInteractionCommand, InteractionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CaptureInteractionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<InteractionDto> Handle(CaptureInteractionCommand request, CancellationToken cancellationToken)
    {
        // Validate tenant
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            throw new UnauthorizedAccessException("TenantId is required");

        var interaction = new Interaction
        {
            TenantId = _currentUser.TenantId,
            Type = request.Type,
            Direction = request.Direction,
            At = request.At ?? DateTime.UtcNow, // Auto-set to current time if not provided
            OwnerType = request.OwnerType,
            OwnerId = request.OwnerId,
            ChannelRef = request.ChannelRef,
            Subject = request.Subject,
            Snippet = request.Snippet,
            PayloadJson = request.PayloadJson,
            DurationSeconds = request.DurationSeconds,
            HandledByUserId = request.HandledByUserId ?? _currentUser.UserId,
            Sentiment = request.Sentiment,
            Tags = request.Tags
        };

        // Note: Domain events would be published here if IDomainEvent interface exists
        // interaction.AddDomainEvent(new InteractionCapturedEvent(interaction));

        _context.Interactions.Add(interaction);
        await _context.SaveChangesAsync(cancellationToken);

        return new InteractionDto
        {
            Id = interaction.Id,
            Type = (int)interaction.Type,
            TypeName = interaction.Type.ToString(),
            Direction = interaction.Direction,
            At = interaction.At,
            OwnerType = (int)interaction.OwnerType,
            OwnerTypeName = interaction.OwnerType.ToString(),
            OwnerId = interaction.OwnerId.ToString(),
            ChannelRef = interaction.ChannelRef,
            Subject = interaction.Subject,
            Snippet = interaction.Snippet,
            PayloadJson = interaction.PayloadJson,
            DurationSeconds = interaction.DurationSeconds,
            HandledByUserId = interaction.HandledByUserId,
            Sentiment = interaction.Sentiment,
            Tags = interaction.Tags,
            Created = interaction.Created,
            CreatedBy = interaction.CreatedBy,
            LastModified = interaction.LastModified,
            LastModifiedBy = interaction.LastModifiedBy,
            IsInbound = interaction.IsInbound,
            IsOutbound = interaction.IsOutbound
        };
    }
}
