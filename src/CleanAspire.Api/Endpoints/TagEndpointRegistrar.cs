// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Features.Tags.Commands;
using CleanAspire.Application.Features.Tags.DTOs;
using CleanAspire.Application.Features.Tags.Queries;
using CleanAspire.Domain.Enums;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace CleanAspire.Api.Endpoints;

/// <summary>
/// Defines minimal API endpoints for tag management.
/// </summary>
public class TagEndpointRegistrar(ILogger<TagEndpointRegistrar> logger) : IEndpointRegistrar
{
    public void RegisterRoutes(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/crm/tags").WithTags("CRM.Tags");

        /// <summary>
        /// Gets all tags (with optional search for autocomplete).
        /// </summary>
        group.MapGet("/", async ([FromServices] IMediator mediator, [FromQuery] string? search) =>
        {
            var query = new GetTagsQuery { Search = search };
            return await mediator.Send(query);
        })
        .Produces<IEnumerable<TagDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Get all tags")
        .WithDescription("Returns a list of all tags, optionally filtered by search term for autocomplete.");

        /// <summary>
        /// Creates a new tag.
        /// </summary>
        group.MapPost("/", ([FromServices] IMediator mediator, [FromBody] CreateTagCommand command) => mediator.Send(command))
            .Produces<TagDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Create a new tag")
            .WithDescription("Creates a new tag with the provided details.");

        /// <summary>
        /// Links a tag to an entity (Client, Contact, etc.).
        /// </summary>
        group.MapPost("/link", ([FromServices] IMediator mediator, [FromBody] LinkTagCommand command) => mediator.Send(command))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Link tag to entity")
            .WithDescription("Links a tag to a specific entity (Client, Contact, Activity, etc.).");

        /// <summary>
        /// Unlinks a tag from an entity.
        /// </summary>
        group.MapPost("/unlink", ([FromServices] IMediator mediator, [FromBody] UnlinkTagCommand command) => mediator.Send(command))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Unlink tag from entity")
            .WithDescription("Removes the link between a tag and an entity.");

        /// <summary>
        /// Gets all tags for a specific entity.
        /// </summary>
        group.MapGet("/{ownerType}/{ownerId}", (
            IMediator mediator,
            [FromRoute] OwnerType ownerType,
            [FromRoute] string ownerId) => mediator.Send(new GetTagsByOwnerQuery(ownerType, ownerId)))
        .Produces<IEnumerable<TagDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Get tags for entity")
        .WithDescription("Returns all tags linked to a specific entity.");
    }
}
