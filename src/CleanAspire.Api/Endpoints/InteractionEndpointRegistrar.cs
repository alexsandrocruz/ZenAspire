using CleanAspire.Application.Common.Models;
using CleanAspire.Application.Features.Interactions.Commands;
using CleanAspire.Application.Features.Interactions.DTOs;
using CleanAspire.Application.Features.Interactions.Queries;
using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Enums;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace CleanAspire.Api.Endpoints;

/// <summary>
/// This class defines minimal API endpoints related to interaction management.
/// Each endpoint corresponds to a specific command or query defined in the Features layer.
/// The purpose is to expose a RESTful interface for capturing and retrieving interactions, delegating request handling to command/query handlers via the Mediator pattern.
/// </summary>
public class InteractionEndpointRegistrar(ILogger<InteractionEndpointRegistrar> logger) : IEndpointRegistrar
{
    /// <summary>
    /// Registers the routes for interaction-related endpoints.
    /// </summary>
    /// <param name="routes">The route builder to which the endpoints will be added.</param>
    public void RegisterRoutes(IEndpointRouteBuilder routes)
    {
        // Primary interaction routes: /api/crm/interactions
        var group = routes.MapGroup("/api/crm/interactions").WithTags("CRM.Interactions"); // .RequireAuthorization(); // TODO: Re-enable authorization after testing

        /// <summary>
        /// Captures a new interaction.
        /// </summary>
        /// <param name="command">The command containing the details of the interaction to capture.</param>
        /// <returns>The captured interaction.</returns>
        group.MapPost("/", ([FromServices] IMediator mediator, [FromBody] CaptureInteractionCommand command) => mediator.Send(command))
        .Produces<InteractionDto>(StatusCodes.Status201Created)
        .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Capture a new interaction")
        .WithDescription("Records a new interaction (email, phone call, WhatsApp message, etc.) with the specified details including type, direction, owner, and content.");

        /// <summary>
        /// Gets an interaction by its ID.
        /// </summary>
        /// <param name="id">The unique ID of the interaction.</param>
        /// <returns>The details of the specified interaction.</returns>
        group.MapGet("/{id}", (IMediator mediator, [FromRoute] string id) => mediator.Send(new GetInteractionByIdQuery(id)))
        .Produces<InteractionDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Get interaction by ID")
        .WithDescription("Returns the details of a specific interaction by its unique ID.");

        /// <summary>
        /// Gets interactions by owner with filtering and pagination.
        /// </summary>
        /// <param name="query">The query containing owner information, filters, and pagination parameters.</param>
        /// <returns>A paginated list of interactions for the specified owner.</returns>
        group.MapPost("/owner", ([FromServices] IMediator mediator, [FromBody] GetInteractionsByOwnerQuery query) => mediator.Send(query))
        .Produces<PaginatedResult<InteractionListDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Get interactions by owner")
        .WithDescription("Returns a paginated list of interactions for a specific owner entity (Client, Contact, etc.) with support for filtering by type, direction, and date range.");

        // Nested routes under clients: /api/crm/clients/{clientId}/interactions
        var clientGroup = routes.MapGroup("/api/crm/clients").WithTags("CRM.Clients"); // .RequireAuthorization(); // TODO: Re-enable authorization after testing

        /// <summary>
        /// Captures a new interaction for a specific client.
        /// </summary>
        /// <param name="clientId">The unique ID of the client.</param>
        /// <param name="command">The command containing the details of the interaction to capture.</param>
        /// <returns>The captured interaction.</returns>
        clientGroup.MapPost("/{clientId}/interactions", (
            IMediator mediator,
            [FromRoute] Guid clientId,
            [FromBody] CaptureInteractionCommand command) =>
                mediator.Send(command with { OwnerType = OwnerType.Client, OwnerId = clientId }))
        .Produces<InteractionDto>(StatusCodes.Status201Created)
        .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Capture interaction for a client")
        .WithDescription("Records a new interaction for a specific client. The owner type and ID are automatically set to the specified client.");

        /// <summary>
        /// Gets interactions for a specific client.
        /// </summary>
        /// <param name="clientId">The unique ID of the client.</param>
        /// <param name="type">Optional filter by interaction type.</param>
        /// <param name="direction">Optional filter by direction (Inbound, Outbound, Internal).</param>
        /// <param name="startDate">Optional start date filter.</param>
        /// <param name="endDate">Optional end date filter.</param>
        /// <param name="pageNumber">Page number (1-based).</param>
        /// <param name="pageSize">Page size.</param>
        /// <returns>A paginated list of interactions for the client.</returns>
        clientGroup.MapGet("/{clientId}/interactions", (
            IMediator mediator,
            [FromRoute] Guid clientId,
            [FromQuery] int? type,
            [FromQuery] string? direction,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10) =>
                mediator.Send(new GetInteractionsByOwnerQuery
                {
                    OwnerType = OwnerType.Client,
                    OwnerId = clientId,
                    Type = type.HasValue ? (InteractionType)type.Value : null,
                    Direction = direction,
                    StartDate = startDate,
                    EndDate = endDate,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                }))
        .Produces<PaginatedResult<InteractionListDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Get client interactions")
        .WithDescription("Returns a paginated list of interactions for a specific client with support for filtering by type, direction, and date range.");
    }
}
