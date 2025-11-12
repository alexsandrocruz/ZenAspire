using CleanAspire.Application.Common.Models;
using CleanAspire.Application.Features.Clients.Commands;
using CleanAspire.Application.Features.Clients.DTOs;
using CleanAspire.Application.Features.Clients.Queries;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace CleanAspire.Api.Endpoints;

/// <summary>
/// This class defines minimal API endpoints related to client management.
/// Each endpoint corresponds to a specific command or query defined in the Features layer.
/// The purpose is to expose a RESTful interface for interacting with clients, delegating request handling to command/query handlers via the Mediator pattern.
/// </summary>
public class ClientEndpointRegistrar(ILogger<ClientEndpointRegistrar> logger) : IEndpointRegistrar
{
    /// <summary>
    /// Registers the routes for client-related endpoints.
    /// </summary>
    /// <param name="routes">The route builder to which the endpoints will be added.</param>
    public void RegisterRoutes(IEndpointRouteBuilder routes)
    {
        // ✅ CRM-aligned route: /api/crm/clients
        var group = routes.MapGroup("/api/crm/clients").WithTags("CRM.Clients").RequireAuthorization();

        /// <summary>
        /// Gets all clients.
        /// </summary>
        /// <returns>A list of all clients in the system.</returns>
        group.MapGet("/", async ([FromServices] IMediator mediator) =>
        {
            var query = new GetAllClientsQuery();
            return await mediator.Send(query);
        })
        .Produces<IEnumerable<ClientDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Get all clients")
        .WithDescription("Returns a list of all clients in the system.");

        /// <summary>
        /// Gets a client by its ID.
        /// </summary>
        /// <param name="id">The unique ID of the client.</param>
        /// <returns>The details of the specified client.</returns>
        group.MapGet("/{id}", (IMediator mediator, [FromRoute] string id) => mediator.Send(new GetClientByIdQuery(id)))
        .Produces<ClientDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Get client by ID")
        .WithDescription("Returns the details of a specific client by its unique ID.");

        /// <summary>
        /// Creates a new client.
        /// </summary>
        /// <param name="command">The command containing the details of the client to create.</param>
        /// <returns>The created client.</returns>
        group.MapPost("/", ([FromServices] IMediator mediator, [FromBody] CreateClientCommand command) => mediator.Send(command))
             .Produces<ClientDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Create a new client")
        .WithDescription("Creates a new client with the provided details.");

        /// <summary>
        /// Updates an existing client.
        /// </summary>
        /// <param name="command">The command containing the updated details of the client.</param>
        group.MapPut("/", ([FromServices] IMediator mediator, [FromBody] UpdateClientCommand command) => mediator.Send(command))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Update an existing client")
        .WithDescription("Updates the details of an existing client.");

        /// <summary>
        /// Deletes a client by its ID.
        /// </summary>
        /// <param name="id">The unique ID of the client to delete.</param>
        group.MapDelete("/{id}", (IMediator mediator, [FromRoute] string id) => mediator.Send(new DeleteClientCommand(id)))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Delete client by ID")
        .WithDescription("Deletes a client by its unique ID.");

        /// <summary>
        /// Gets clients with pagination and filtering.
        /// </summary>
        /// <param name="query">The query containing pagination and filtering parameters.</param>
        /// <returns>A paginated list of clients.</returns>
        group.MapPost("/pagination", ([FromServices] IMediator mediator, [FromBody] ClientsWithPaginationQuery query) => mediator.Send(query))
        .Produces<PaginatedResult<ClientDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Get clients with pagination")
        .WithDescription("Returns a paginated list of clients based on search keywords, page size, and sorting options.");
    }
}