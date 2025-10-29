using CleanAspire.Application.Common.Models;
using CleanAspire.Application.Features.Contacts.Commands;
using CleanAspire.Application.Features.Contacts.DTOs;
using CleanAspire.Application.Features.Contacts.Queries;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace CleanAspire.Api.Endpoints;

/// <summary>
/// This class defines minimal API endpoints related to contact management.
/// Each endpoint corresponds to a specific command or query defined in the Features layer.
/// The purpose is to expose a RESTful interface for interacting with contacts, delegating request handling to command/query handlers via the Mediator pattern.
/// </summary>
public class ContactEndpointRegistrar(ILogger<ContactEndpointRegistrar> logger) : IEndpointRegistrar
{
    /// <summary>
    /// Registers the routes for contact-related endpoints.
    /// </summary>
    /// <param name="routes">The route builder to which the endpoints will be added.</param>
    public void RegisterRoutes(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/contacts").WithTags("contacts").RequireAuthorization();

        /// <summary>
        /// Gets all contacts.
        /// </summary>
        /// <returns>A list of all contacts in the system.</returns>
        group.MapGet("/", async ([FromServices] IMediator mediator) =>
        {
            var query = new GetAllContactsQuery();
            return await mediator.Send(query);
        })
        .Produces<IEnumerable<ContactDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Get all contacts")
        .WithDescription("Returns a list of all contacts in the system.");

        /// <summary>
        /// Gets a contact by its ID.
        /// </summary>
        /// <param name="id">The unique ID of the contact.</param>
        /// <returns>The details of the specified contact.</returns>
        group.MapGet("/{id}", (IMediator mediator, [FromRoute] string id) => mediator.Send(new GetContactByIdQuery(id)))
        .Produces<ContactDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Get contact by ID")
        .WithDescription("Returns the details of a specific contact by its unique ID.");

        /// <summary>
        /// Gets contacts by client ID.
        /// </summary>
        /// <param name="clientId">The unique ID of the client.</param>
        /// <returns>A list of contacts for the specified client.</returns>
        group.MapGet("/by-client/{clientId}", (IMediator mediator, [FromRoute] string clientId) => mediator.Send(new GetContactsByClientIdQuery(clientId)))
        .Produces<IEnumerable<ContactDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Get contacts by client ID")
        .WithDescription("Returns a list of contacts for a specific client.");

        /// <summary>
        /// Creates a new contact.
        /// </summary>
        /// <param name="command">The command containing the details of the contact to create.</param>
        /// <returns>The created contact.</returns>
        group.MapPost("/", ([FromServices] IMediator mediator, [FromBody] CreateContactCommand command) => mediator.Send(command))
             .Produces<ContactDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Create a new contact")
        .WithDescription("Creates a new contact with the provided details.");

        /// <summary>
        /// Updates an existing contact.
        /// </summary>
        /// <param name="command">The command containing the updated details of the contact.</param>
        group.MapPut("/", ([FromServices] IMediator mediator, [FromBody] UpdateContactCommand command) => mediator.Send(command))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Update an existing contact")
        .WithDescription("Updates the details of an existing contact.");

        /// <summary>
        /// Deletes a contact by its ID.
        /// </summary>
        /// <param name="id">The unique ID of the contact to delete.</param>
        group.MapDelete("/{id}", (IMediator mediator, [FromRoute] string id) => mediator.Send(new DeleteContactCommand(id)))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Delete contact by ID")
        .WithDescription("Deletes a contact by its unique ID.");

        /// <summary>
        /// Gets contacts with pagination and filtering.
        /// </summary>
        /// <param name="query">The query containing pagination and filtering parameters.</param>
        /// <returns>A paginated list of contacts.</returns>
        group.MapPost("/pagination", ([FromServices] IMediator mediator, [FromBody] ContactsWithPaginationQuery query) => mediator.Send(query))
        .Produces<PaginatedResult<ContactDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Get contacts with pagination")
        .WithDescription("Returns a paginated list of contacts based on search keywords, page size, and sorting options.");
    }
}