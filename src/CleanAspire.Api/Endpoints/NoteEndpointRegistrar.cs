// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Common.Models;
using CleanAspire.Application.Features.Notes.Commands;
using CleanAspire.Application.Features.Notes.DTOs;
using CleanAspire.Application.Features.Notes.Queries;
using CleanAspire.Domain.Enums;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace CleanAspire.Api.Endpoints;

/// <summary>
/// Defines minimal API endpoints for note management.
/// </summary>
public class NoteEndpointRegistrar(ILogger<NoteEndpointRegistrar> logger) : IEndpointRegistrar
{
    public void RegisterRoutes(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/crm/notes").WithTags("CRM.Notes");

        /// <summary>
        /// Gets paginated notes for a specific entity.
        /// </summary>
        group.MapGet("/owner/{ownerType}/{ownerId}", (
            IMediator mediator,
            [FromRoute] OwnerType ownerType,
            [FromRoute] Guid ownerId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10) => mediator.Send(new GetNotesByOwnerQuery(ownerType, ownerId.ToString())))
        .Produces<IEnumerable<NoteDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Get notes for entity")
        .WithDescription("Returns a list of notes for a specific entity (Client, Contact, etc.).");

        /// <summary>
        /// Gets all notes for a specific entity (legacy endpoint for backward compatibility).
        /// </summary>
        group.MapGet("/{ownerType}/{ownerId}", (
            IMediator mediator,
            [FromRoute] OwnerType ownerType,
            [FromRoute] string ownerId) => mediator.Send(new GetNotesByOwnerQuery(ownerType, ownerId)))
        .Produces<IEnumerable<NoteDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Get notes for entity")
        .WithDescription("Returns all notes for a specific entity (Client, Contact, etc.).");

        /// <summary>
        /// Creates a new note.
        /// </summary>
        group.MapPost("/", ([FromServices] IMediator mediator, [FromBody] CreateNoteCommand command) => mediator.Send(command))
            .Produces<NoteDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Create a new note")
            .WithDescription("Creates a new note with the provided details.");

        /// <summary>
        /// Updates an existing note.
        /// </summary>
        group.MapPut("/", ([FromServices] IMediator mediator, [FromBody] UpdateNoteCommand command) => mediator.Send(command))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Update an existing note")
            .WithDescription("Updates the details of an existing note.");

        /// <summary>
        /// Deletes a note by its ID.
        /// </summary>
        group.MapDelete("/{id}", (IMediator mediator, [FromRoute] string id) => mediator.Send(new DeleteNoteCommand(id)))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Delete note by ID")
            .WithDescription("Deletes a note by its unique ID.");
    }
}
