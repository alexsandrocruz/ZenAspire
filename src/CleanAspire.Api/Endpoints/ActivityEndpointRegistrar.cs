using CleanAspire.Application.Common.Models;
using CleanAspire.Application.Features.Activities.Commands;
using CleanAspire.Application.Features.Activities.DTOs;
using CleanAspire.Application.Features.Activities.Queries;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace CleanAspire.Api.Endpoints;

/// <summary>
/// This class defines minimal API endpoints related to activity management.
/// Each endpoint corresponds to a specific command or query defined in the Features layer.
/// The purpose is to expose a RESTful interface for interacting with activities, delegating request handling to command/query handlers via the Mediator pattern.
/// </summary>
public class ActivityEndpointRegistrar(ILogger<ActivityEndpointRegistrar> logger) : IEndpointRegistrar
{
    /// <summary>
    /// Registers the routes for activity-related endpoints.
    /// </summary>
    /// <param name="routes">The route builder to which the endpoints will be added.</param>
    public void RegisterRoutes(IEndpointRouteBuilder routes)
    {
        // CRM-aligned route: /api/crm/activities
        var group = routes.MapGroup("/api/crm/activities").WithTags("CRM.Activities"); // .RequireAuthorization(); // TODO: Re-enable authorization after testing

        /// <summary>
        /// Gets activities with filtering, pagination, and sorting.
        /// </summary>
        /// <returns>A paginated list of activities with applied filters.</returns>
        group.MapPost("/search", ([FromServices] IMediator mediator, [FromBody] GetActivitiesQuery query) => mediator.Send(query))
        .Produces<PaginatedResult<ActivityListDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Get activities with filtering")
        .WithDescription("Returns a paginated list of activities with support for filtering by status, type, priority, assigned user, regarding entity, date ranges, and keyword search.");

        /// <summary>
        /// Gets an activity by its ID.
        /// </summary>
        /// <param name="id">The unique ID of the activity.</param>
        /// <returns>The details of the specified activity.</returns>
        group.MapGet("/{id}", (IMediator mediator, [FromRoute] string id) => mediator.Send(new GetActivityByIdQuery(id)))
        .Produces<ActivityDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Get activity by ID")
        .WithDescription("Returns the details of a specific activity by its unique ID.");

        /// <summary>
        /// Creates a new activity.
        /// </summary>
        /// <param name="command">The command containing the details of the activity to create.</param>
        /// <returns>The created activity.</returns>
        group.MapPost("/", ([FromServices] IMediator mediator, [FromBody] CreateActivityCommand command) => mediator.Send(command))
        .Produces<ActivityDto>(StatusCodes.Status201Created)
        .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Create a new activity")
        .WithDescription("Creates a new activity with the provided details including type, status, dates, subject, and assignment information.");

        /// <summary>
        /// Updates an existing activity.
        /// </summary>
        /// <param name="command">The command containing the updated details of the activity.</param>
        group.MapPut("/", ([FromServices] IMediator mediator, [FromBody] UpdateActivityCommand command) => mediator.Send(command))
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Update an existing activity")
        .WithDescription("Updates the details of an existing activity including type, status, dates, subject, description, and assignment.");

        /// <summary>
        /// Deletes an activity by its ID.
        /// </summary>
        /// <param name="id">The unique ID of the activity to delete.</param>
        group.MapDelete("/{id}", (IMediator mediator, [FromRoute] string id) => mediator.Send(new DeleteActivityCommand { Id = id }))
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Delete activity by ID")
        .WithDescription("Permanently removes an activity by its unique ID.");

        /// <summary>
        /// Marks an activity as completed.
        /// </summary>
        /// <param name="id">The unique ID of the activity to complete.</param>
        /// <param name="command">The command containing optional completion notes.</param>
        group.MapPost("/{id}/complete", (IMediator mediator, [FromRoute] string id, [FromBody] CompleteActivityCommand command) => mediator.Send(command with { Id = id }))
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Complete an activity")
        .WithDescription("Marks an activity as completed, sets the completion timestamp, and optionally adds completion notes.");

        /// <summary>
        /// Gets activities assigned to a specific user.
        /// </summary>
        /// <param name="userId">The user ID (optional - defaults to current user if not provided).</param>
        /// <param name="query">The query containing filtering and pagination parameters.</param>
        /// <returns>A paginated list of activities for the user.</returns>
        group.MapPost("/user/{userId?}", (
            IMediator mediator,
            [FromRoute] string? userId,
            [FromBody] GetActivitiesByUserQuery query) =>
                mediator.Send(query with { UserId = userId ?? query.UserId }))
        .Produces<PaginatedResult<ActivityListDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Get activities by user")
        .WithDescription("Returns a paginated list of activities assigned to a specific user. If no user ID is provided, returns activities for the current user.");
    }
}
