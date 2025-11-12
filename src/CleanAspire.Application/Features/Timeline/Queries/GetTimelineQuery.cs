// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Common.Interfaces;
using CleanAspire.Application.Common.Models;
using CleanAspire.Application.Features.Timeline.DTOs;
using CleanAspire.Application.Pipeline;
using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Enums;

namespace CleanAspire.Application.Features.Timeline.Queries;

/// <summary>
/// Query for getting a unified timeline of Activities, Interactions, and Notes
/// for a specific owner entity (Client, Contact, etc.).
/// </summary>
public record GetTimelineQuery : IFusionCacheRequest<PaginatedResult<TimelineItemDto>>
{
    /// <summary>
    /// Type of entity (Client, Contact, etc.) - Required
    /// </summary>
    public OwnerType OwnerType { get; init; }

    /// <summary>
    /// ID of the entity - Required
    /// </summary>
    public Guid OwnerId { get; init; }

    /// <summary>
    /// Filter by start date (optional)
    /// </summary>
    public DateTime? StartDate { get; init; }

    /// <summary>
    /// Filter by end date (optional)
    /// </summary>
    public DateTime? EndDate { get; init; }

    /// <summary>
    /// Filter by timeline item type (Activity, Interaction, Note) - optional
    /// </summary>
    public TimelineItemType? TypeFilter { get; init; }

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; init; } = 20;

    /// <summary>
    /// Tags for cache invalidation
    /// </summary>
    public IEnumerable<string>? Tags => new[] { "timeline", "activities", "interactions", "notes" };
}

/// <summary>
/// Handler for processing GetTimelineQuery.
/// Merges Activities, Interactions, and Notes into a unified timeline view.
/// </summary>
public class GetTimelineQueryHandler : IRequestHandler<GetTimelineQuery, PaginatedResult<TimelineItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetTimelineQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<PaginatedResult<TimelineItemDto>> Handle(GetTimelineQuery request, CancellationToken cancellationToken)
    {
        // Validate tenant
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            throw new UnauthorizedAccessException("TenantId is required");

        // Build list of timeline items from different sources
        var timelineItems = new List<TimelineItemDto>();

        // Include Activities if not filtered out
        if (!request.TypeFilter.HasValue || request.TypeFilter == TimelineItemType.Activity)
        {
            var activities = await GetActivitiesAsync(request, cancellationToken);
            timelineItems.AddRange(activities);
        }

        // Include Interactions if not filtered out
        if (!request.TypeFilter.HasValue || request.TypeFilter == TimelineItemType.Interaction)
        {
            var interactions = await GetInteractionsAsync(request, cancellationToken);
            timelineItems.AddRange(interactions);
        }

        // Include Notes if not filtered out
        if (!request.TypeFilter.HasValue || request.TypeFilter == TimelineItemType.Note)
        {
            var notes = await GetNotesAsync(request, cancellationToken);
            timelineItems.AddRange(notes);
        }

        // Sort by date descending (most recent first)
        var sortedItems = timelineItems.OrderByDescending(item => item.Date).ToList();

        // Get total count before pagination
        var totalCount = sortedItems.Count;

        // Apply pagination
        var paginatedItems = sortedItems
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PaginatedResult<TimelineItemDto>(
            paginatedItems,
            totalCount,
            request.PageNumber,
            request.PageSize);
    }

    /// <summary>
    /// Retrieves activities for the timeline.
    /// </summary>
    private async Task<List<TimelineItemDto>> GetActivitiesAsync(GetTimelineQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Activities
            .Where(a => a.TenantId == _currentUser.TenantId);

        // If filtering by specific entity, include both entity-specific and general activities
        if (request.OwnerType != default && request.OwnerId != Guid.Empty)
        {
            // Map OwnerType to RegardingType
            // OwnerType and RegardingType have similar values (Client=1, Contact=2, etc.)
            var regardingType = (RegardingType)(int)request.OwnerType;

            query = query.Where(a =>
                // Activities specifically for this entity
                (a.RegardingType == regardingType && a.RegardingId == request.OwnerId) ||
                // OR general activities not associated with any entity
                (a.RegardingType == null || a.RegardingId == null));
        }

        // Apply date range filter
        if (request.StartDate.HasValue)
        {
            query = query.Where(a => a.Start >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(a => a.Start <= request.EndDate.Value);
        }

        var activities = await query
            .Select(a => new TimelineItemDto
            {
                Id = a.Id,
                Type = TimelineItemType.Activity,
                Date = a.Start,
                Title = a.Subject,
                Description = a.Description,
                Icon = GetActivityIcon(a.Type),
                Badge = a.Status.ToString(),
                ColorHint = GetActivityColorHint(a.Status),
                TypeCode = ((int)a.Type).ToString(),
                TypeName = a.Type.ToString(),
                Status = a.Status.ToString(),
                Duration = a.DurationMinutes,
                User = a.AssignedToUserId,
                IsPinned = false,
                IsPrivate = false,
                CreatedAt = a.Created,
                CreatedBy = a.CreatedBy
            })
            .ToListAsync(cancellationToken);

        return activities;
    }

    /// <summary>
    /// Retrieves interactions for the timeline.
    /// </summary>
    private async Task<List<TimelineItemDto>> GetInteractionsAsync(GetTimelineQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Interactions
            .Where(i => i.TenantId == _currentUser.TenantId)
            .Where(i => i.OwnerType == request.OwnerType && i.OwnerId == request.OwnerId);

        // Apply date range filter
        if (request.StartDate.HasValue)
        {
            query = query.Where(i => i.At >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(i => i.At <= request.EndDate.Value);
        }

        var interactions = await query
            .Select(i => new TimelineItemDto
            {
                Id = i.Id,
                Type = TimelineItemType.Interaction,
                Date = i.At,
                Title = i.Subject ?? i.Type.ToString(),
                Description = i.Snippet,
                Icon = GetInteractionIcon(i.Type),
                Badge = i.Direction,
                ColorHint = GetInteractionColorHint(i.Direction),
                TypeCode = ((int)i.Type).ToString(),
                TypeName = i.Type.ToString(),
                Status = i.Direction,
                Duration = i.DurationSeconds,
                User = i.HandledByUserId,
                Metadata = i.PayloadJson,
                IsPinned = false,
                IsPrivate = false,
                CreatedAt = i.Created,
                CreatedBy = i.CreatedBy
            })
            .ToListAsync(cancellationToken);

        return interactions;
    }

    /// <summary>
    /// Retrieves notes for the timeline.
    /// </summary>
    private async Task<List<TimelineItemDto>> GetNotesAsync(GetTimelineQuery request, CancellationToken cancellationToken)
    {
        var ownerIdStr = request.OwnerId.ToString();

        var query = _context.Notes
            .Where(n => n.TenantId == _currentUser.TenantId)
            .Where(n => n.OwnerType == request.OwnerType && n.OwnerId == ownerIdStr);

        // Apply date range filter using Created date for notes
        if (request.StartDate.HasValue)
        {
            query = query.Where(n => n.Created >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(n => n.Created <= request.EndDate.Value);
        }

        var notes = await query
            .Select(n => new TimelineItemDto
            {
                Id = n.Id,
                Type = TimelineItemType.Note,
                Date = n.Created ?? DateTime.UtcNow, // Use Created date, fallback to UtcNow
                Title = n.Title ?? "Note",
                Description = n.Body.Length > 500 ? n.Body.Substring(0, 500) + "..." : n.Body,
                Icon = "note",
                Badge = n.IsPinned ? "Pinned" : null,
                ColorHint = n.IsPinned ? "warning" : "default",
                TypeCode = "Note",
                TypeName = "Note",
                Status = null,
                Duration = null,
                User = n.CreatedBy,
                IsPinned = n.IsPinned,
                IsPrivate = n.IsPrivate,
                CreatedAt = n.Created,
                CreatedBy = n.CreatedBy
            })
            .ToListAsync(cancellationToken);

        return notes;
    }

    /// <summary>
    /// Maps activity type to an icon identifier.
    /// </summary>
    private static string GetActivityIcon(ActivityType type)
    {
        return type switch
        {
            ActivityType.Task => "task",
            ActivityType.PhoneCall => "phone",
            ActivityType.Meeting => "meeting",
            ActivityType.Email => "email",
            ActivityType.WhatsAppMessage => "whatsapp",
            ActivityType.WhatsAppFlow => "whatsapp",
            ActivityType.SMS => "sms",
            ActivityType.VideoCall => "video",
            ActivityType.Event => "event",
            _ => "activity"
        };
    }

    /// <summary>
    /// Maps activity status to a color hint for UI rendering.
    /// </summary>
    private static string GetActivityColorHint(ActivityStatus status)
    {
        return status switch
        {
            ActivityStatus.Open => "info",
            ActivityStatus.InProgress => "warning",
            ActivityStatus.Completed => "success",
            ActivityStatus.Canceled => "error",
            ActivityStatus.Deferred => "default",
            _ => "default"
        };
    }

    /// <summary>
    /// Maps interaction type to an icon identifier.
    /// </summary>
    private static string GetInteractionIcon(InteractionType type)
    {
        return type switch
        {
            InteractionType.Email => "email",
            InteractionType.WhatsApp => "whatsapp",
            InteractionType.PhoneCall => "phone",
            InteractionType.SMS => "sms",
            InteractionType.Meeting => "meeting",
            InteractionType.VideoCall => "video",
            InteractionType.WebChat => "chat",
            InteractionType.InstagramDM => "instagram",
            InteractionType.FacebookMessage => "facebook",
            InteractionType.LinkedInMessage => "linkedin",
            InteractionType.TwitterDM => "twitter",
            _ => "interaction"
        };
    }

    /// <summary>
    /// Maps interaction direction to a color hint for UI rendering.
    /// </summary>
    private static string GetInteractionColorHint(string direction)
    {
        return direction switch
        {
            InteractionDirection.Inbound => "success",
            InteractionDirection.Outbound => "info",
            InteractionDirection.Internal => "default",
            _ => "default"
        };
    }
}
