using CleanAspire.Application.Features.Segments.Commands;
using CleanAspire.Application.Features.Segments.DTOs;
using CleanAspire.Application.Features.Segments.Queries;
using CleanAspire.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanAspire.Api.Endpoints.Segments;

/// <summary>
/// API endpoints for segment management
/// Part of Phase 5: Segmentation Engine
/// </summary>
public class SegmentEndpoints : IEndpointRegistrar
{
    public void RegisterRoutes(IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/segments")
            .WithTags("Segments")
            .RequireAuthorization();

        // GET /api/segments - Get all segments for current tenant
        group.MapGet("/", GetAllSegmentsAsync)
            .WithName("GetAllSegments")
            .WithSummary("Get all segments")
            .WithDescription("Get all segments for the current tenant")
            .Produces<List<SegmentDto>>()
            .Produces(401);

        // GET /api/segments/{id} - Get segment by id
        group.MapGet("/{id:guid}", GetSegmentByIdAsync)
            .WithName("GetSegmentById")
            .WithSummary("Get segment by ID")
            .WithDescription("Get a specific segment by ID")
            .Produces<SegmentDto>()
            .Produces(404)
            .Produces(401);

        // GET /api/segments/{id}/stats - Get segment statistics
        group.MapGet("/{id:guid}/stats", GetSegmentStatsAsync)
            .WithName("GetSegmentStats")
            .WithSummary("Get segment statistics")
            .WithDescription("Get detailed statistics for a segment")
            .Produces<SegmentStatsDto>()
            .Produces(404)
            .Produces(401);

        // GET /api/segments/{id}/members - Get segment members
        group.MapGet("/{id:guid}/members", GetSegmentMembersAsync)
            .WithName("GetSegmentMembers")
            .WithSummary("Get segment members")
            .WithDescription("Get all members of a segment with pagination")
            .Produces<SegmentMembershipListDto>()
            .Produces(404)
            .Produces(401);

        // POST /api/segments - Create new segment
        group.MapPost("/", CreateSegmentAsync)
            .WithName("CreateSegment")
            .WithSummary("Create new segment")
            .WithDescription("Create a new segment with rule definition")
            .Accepts<CreateSegmentCommand>("application/json")
            .Produces<SegmentDto>(201)
            .Produces(400)
            .Produces(401)
            .ProducesValidationProblem();

        // PUT /api/segments/{id} - Update segment
        group.MapPut("/{id:guid}", UpdateSegmentAsync)
            .WithName("UpdateSegment")
            .WithSummary("Update segment")
            .WithDescription("Update an existing segment")
            .Accepts<UpdateSegmentCommand>("application/json")
            .Produces<SegmentDto>()
            .Produces(400)
            .Produces(404)
            .Produces(401)
            .ProducesValidationProblem();

        // DELETE /api/segments/{id} - Delete segment
        group.MapDelete("/{id:guid}", DeleteSegmentAsync)
            .WithName("DeleteSegment")
            .WithSummary("Delete segment")
            .WithDescription("Delete a segment (soft delete)")
            .Produces(204)
            .Produces(404)
            .Produces(401);

        // POST /api/segments/{id}/rebuild - Rebuild segment
        group.MapPost("/{id:guid}/rebuild", RebuildSegmentAsync)
            .WithName("RebuildSegment")
            .WithSummary("Rebuild segment")
            .WithDescription("Trigger a rebuild of segment membership")
            .Produces<SegmentStatsDto>()
            .Produces(400)
            .Produces(404)
            .Produces(401);

        // POST /api/segments/rebuild-all - Rebuild all active segments
        group.MapPost("/rebuild-all", RebuildAllSegmentsAsync)
            .WithName("RebuildAllSegments")
            .WithSummary("Rebuild all segments")
            .WithDescription("Trigger rebuild for all active segments (admin endpoint)")
            .Produces<List<SegmentRebuildResultDto>>()
            .Produces(401)
            .RequireAuthorization("Admin");

        // GET /api/segments/fields - Get available fields for segment rules
        group.MapGet("/fields", GetAvailableFieldsAsync)
            .WithName("GetSegmentFields")
            .WithSummary("Get available fields")
            .WithDescription("Get available fields for segment rule definitions")
            .Produces<SegmentFieldsResponseDto>()
            .Produces(401);

        // POST /api/segments/validate - Validate segment definition
        group.MapPost("/validate", ValidateSegmentDefinitionAsync)
            .WithName("ValidateSegmentDefinition")
            .WithSummary("Validate segment definition")
            .WithDescription("Validate a segment rule definition without creating a segment")
            .Accepts<ValidateSegmentDefinitionCommand>("application/json")
            .Produces<SegmentValidationResultDto>()
            .Produces(400)
            .ProducesValidationProblem();
    }

    private static async Task<IResult> GetAllSegmentsAsync(
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllSegmentsQuery();
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetSegmentByIdAsync(
        Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSegmentByIdQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        if (result == null)
            return Results.NotFound();

        return Results.Ok(result);
    }

    private static async Task<IResult> GetSegmentStatsAsync(
        Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSegmentStatsQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        if (result == null)
            return Results.NotFound();

        return Results.Ok(result);
    }

    private static async Task<IResult> GetSegmentMembersAsync(
        Guid id,
        [FromServices] IMediator mediator,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] OwnerType? ownerType = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSegmentMembersListQuery
        {
            Id = id,
            Page = page,
            PageSize = pageSize,
            OwnerType = ownerType
        };
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> CreateSegmentAsync(
        [FromServices] IMediator mediator,
        [FromBody] CreateSegmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(command, cancellationToken);
        var segmentDto = (SegmentDto)result;
        return Results.Created($"/api/segments/{segmentDto.Id}", segmentDto);
    }

    private static async Task<IResult> UpdateSegmentAsync(
        Guid id,
        [FromServices] IMediator mediator,
        [FromBody] UpdateSegmentCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.Id != id)
            return Results.BadRequest("ID in URL must match ID in request body");

        var result = await mediator.Send(command, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> DeleteSegmentAsync(
        Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteSegmentCommand { Id = id };
        await mediator.Send(command, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> RebuildSegmentAsync(
        Guid id,
        [FromServices] IMediator mediator,
        [FromQuery] bool force = false,
        CancellationToken cancellationToken = default)
    {
        var command = new RebuildSegmentCommand
        {
            Id = id,
            ForceRebuild = force
        };
        var result = await mediator.Send(command, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> RebuildAllSegmentsAsync(
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var command = new RebuildAllSegmentsCommand();
        var result = await mediator.Send(command, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetAvailableFieldsAsync(
        [FromServices] IMediator mediator,
        [FromQuery] OwnerType? ownerType = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSegmentFieldsQuery { OwnerType = ownerType };
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> ValidateSegmentDefinitionAsync(
        [FromServices] IMediator mediator,
        [FromBody] ValidateSegmentDefinitionCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(command, cancellationToken);
        return Results.Ok(result);
    }
}