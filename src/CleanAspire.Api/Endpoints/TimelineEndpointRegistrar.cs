using CleanAspire.Application.Common.Models;
using CleanAspire.Application.Features.Timeline.DTOs;
using CleanAspire.Application.Features.Timeline.Queries;
using CleanAspire.Domain.Enums;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace CleanAspire.Api.Endpoints;

/// <summary>
/// This class defines minimal API endpoints related to timeline functionality.
/// Each endpoint corresponds to a specific query defined in the Features layer.
/// The purpose is to expose a RESTful interface for retrieving unified timelines of Activities, Interactions, and Notes, delegating request handling to query handlers via the Mediator pattern.
/// </summary>
public class TimelineEndpointRegistrar(ILogger<TimelineEndpointRegistrar> logger) : IEndpointRegistrar
{
    /// <summary>
    /// Registers the routes for timeline-related endpoints.
    /// </summary>
    /// <param name="routes">The route builder to which the endpoints will be added.</param>
    public void RegisterRoutes(IEndpointRouteBuilder routes)
    {
        // Global timeline routes: /api/crm/timeline
        var globalGroup = routes.MapGroup("/api/crm/timeline").WithTags("CRM.Timeline"); // .RequireAuthorization(); // TODO: Re-enable authorization after testing

        /// <summary>
        /// Gets the global unified timeline across all entities.
        /// </summary>
        /// <param name="ownerType">Optional filter by owner type (Client, Contact).</param>
        /// <param name="ownerId">Optional filter by specific owner ID.</param>
        /// <param name="startDate">Optional start date filter.</param>
        /// <param name="endDate">Optional end date filter.</param>
        /// <param name="typeFilter">Optional filter by timeline item type (Activity, Interaction, Note).</param>
        /// <param name="pageNumber">Page number (1-based).</param>
        /// <param name="pageSize">Page size.</param>
        /// <returns>A paginated unified timeline of activities, interactions, and notes.</returns>
        globalGroup.MapGet("/", async (
            IMediator mediator,
            [FromQuery] OwnerType? ownerType,
            [FromQuery] Guid? ownerId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int? typeFilter,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20) =>
        {
            // If no ownerType/ownerId specified, we'll need to create a different query for global timeline
            // For now, return empty result for global timeline
            // TODO: Implement GetGlobalTimelineQuery for truly global timeline
            if (!ownerType.HasValue || !ownerId.HasValue)
            {
                return new PaginatedResult<TimelineItemDto>(
                    Enumerable.Empty<TimelineItemDto>(),
                    0,
                    pageNumber,
                    pageSize
                );
            }

            return await mediator.Send(new GetTimelineQuery
            {
                OwnerType = ownerType.Value,
                OwnerId = ownerId.Value,
                StartDate = startDate,
                EndDate = endDate,
                TypeFilter = typeFilter.HasValue ? (TimelineItemType)typeFilter.Value : null,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        })
        .Produces<PaginatedResult<TimelineItemDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Get global timeline")
        .WithDescription("Returns a unified timeline of activities, interactions, and notes across all entities with optional filtering by owner type and ID.");

        // Timeline routes nested under clients: /api/crm/clients/{clientId}/timeline
        var clientGroup = routes.MapGroup("/api/crm/clients").WithTags("CRM.Clients"); // .RequireAuthorization(); // TODO: Re-enable authorization after testing

        /// <summary>
        /// Gets the unified timeline for a specific client.
        /// </summary>
        /// <param name="clientId">The unique ID of the client.</param>
        /// <param name="startDate">Optional start date filter.</param>
        /// <param name="endDate">Optional end date filter.</param>
        /// <param name="typeFilter">Optional filter by timeline item type (Activity, Interaction, Note).</param>
        /// <param name="pageNumber">Page number (1-based).</param>
        /// <param name="pageSize">Page size.</param>
        /// <returns>A paginated unified timeline of activities, interactions, and notes for the client.</returns>
        clientGroup.MapGet("/{clientId}/timeline", (
            IMediator mediator,
            [FromRoute] Guid clientId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int? typeFilter,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20) =>
                mediator.Send(new GetTimelineQuery
                {
                    OwnerType = OwnerType.Client,
                    OwnerId = clientId,
                    StartDate = startDate,
                    EndDate = endDate,
                    TypeFilter = typeFilter.HasValue ? (TimelineItemType)typeFilter.Value : null,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                }))
        .Produces<PaginatedResult<TimelineItemDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Get client timeline")
        .WithDescription("Returns a unified timeline of activities, interactions, and notes for a specific client with support for filtering by date range and item type.");

        // Timeline routes nested under contacts: /api/crm/contacts/{contactId}/timeline
        var contactGroup = routes.MapGroup("/api/crm/contacts").WithTags("CRM.Contacts"); // .RequireAuthorization(); // TODO: Re-enable authorization after testing

        /// <summary>
        /// Gets the unified timeline for a specific contact.
        /// </summary>
        /// <param name="contactId">The unique ID of the contact.</param>
        /// <param name="startDate">Optional start date filter.</param>
        /// <param name="endDate">Optional end date filter.</param>
        /// <param name="typeFilter">Optional filter by timeline item type (Activity, Interaction, Note).</param>
        /// <param name="pageNumber">Page number (1-based).</param>
        /// <param name="pageSize">Page size.</param>
        /// <returns>A paginated unified timeline of activities, interactions, and notes for the contact.</returns>
        contactGroup.MapGet("/{contactId}/timeline", (
            IMediator mediator,
            [FromRoute] Guid contactId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int? typeFilter,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20) =>
                mediator.Send(new GetTimelineQuery
                {
                    OwnerType = OwnerType.Contact,
                    OwnerId = contactId,
                    StartDate = startDate,
                    EndDate = endDate,
                    TypeFilter = typeFilter.HasValue ? (TimelineItemType)typeFilter.Value : null,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                }))
        .Produces<PaginatedResult<TimelineItemDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Get contact timeline")
        .WithDescription("Returns a unified timeline of activities, interactions, and notes for a specific contact with support for filtering by date range and item type.");
    }
}
