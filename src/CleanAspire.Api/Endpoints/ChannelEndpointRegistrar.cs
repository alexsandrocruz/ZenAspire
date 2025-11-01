// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Features.Channels.Commands;
using CleanAspire.Application.Features.Channels.DTOs;
using CleanAspire.Application.Features.Channels.Queries;
using CleanAspire.Domain.Enums;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace CleanAspire.Api.Endpoints;

/// <summary>
/// API endpoints for ChannelIdentity management
/// </summary>
public class ChannelEndpointRegistrar : IEndpointRegistrar
{
    public void RegisterRoutes(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/crm/channels")
            .WithTags("CRM.Channels");
            // .RequireAuthorization(); // TODO: Enable after testing

        // GET /api/crm/channels/{id}
        group.MapGet("/{id}", GetChannelById)
            .WithName("GetChannelById")
            .WithSummary("Get channel identity by ID")
            .Produces<ChannelIdentityDto>()
            .Produces(404);

        // GET /api/crm/channels/owner/{ownerType}/{ownerId}
        group.MapGet("/owner/{ownerType}/{ownerId}", GetChannelsByOwner)
            .WithName("GetChannelsByOwner")
            .WithSummary("Get all channel identities for an owner (Client or Contact)")
            .Produces<List<ChannelIdentityDto>>();

        // GET /api/crm/channels/owner/{ownerType}/{ownerId}/type/{channelType}
        group.MapGet("/owner/{ownerType}/{ownerId}/type/{channelType}", GetChannelsByType)
            .WithName("GetChannelsByType")
            .WithSummary("Get channel identities by owner and channel type")
            .Produces<List<ChannelIdentityDto>>();

        // GET /api/crm/channels/owner/{ownerType}/{ownerId}/primary
        group.MapGet("/owner/{ownerType}/{ownerId}/primary", GetPrimaryChannels)
            .WithName("GetPrimaryChannels")
            .WithSummary("Get all primary channel identities for an owner")
            .Produces<List<ChannelIdentityDto>>();

        // POST /api/crm/channels
        group.MapPost("/", CreateChannel)
            .WithName("CreateChannel")
            .WithSummary("Create a new channel identity")
            .Produces<ChannelIdentityDto>(201)
            .ProducesValidationProblem();

        // PUT /api/crm/channels
        group.MapPut("/", UpdateChannel)
            .WithName("UpdateChannel")
            .WithSummary("Update an existing channel identity")
            .Produces(204)
            .Produces(404)
            .ProducesValidationProblem();

        // DELETE /api/crm/channels/{id}
        group.MapDelete("/{id}", DeleteChannel)
            .WithName("DeleteChannel")
            .WithSummary("Delete a channel identity")
            .Produces(204)
            .Produces(404);

        // POST /api/crm/channels/{id}/verify
        group.MapPost("/{id}/verify", VerifyChannel)
            .WithName("VerifyChannel")
            .WithSummary("Mark a channel as verified")
            .Produces(204)
            .Produces(404);

        // POST /api/crm/channels/{id}/opt-in
        group.MapPost("/{id}/opt-in", UpdateOptIn)
            .WithName("UpdateOptIn")
            .WithSummary("Update opt-in status for a channel (LGPD compliance)")
            .Produces(204)
            .Produces(404);
    }

    private static async Task<IResult> GetChannelById(
        string id,
        ISender sender)
    {
        var query = new GetChannelByIdQuery(id);
        var result = await sender.Send(query);
        return result is not null ? Results.Ok(result) : Results.NotFound();
    }

    private static async Task<IResult> GetChannelsByOwner(
        OwnerType ownerType,
        string ownerId,
        ISender sender)
    {
        var query = new GetChannelsByOwnerQuery
        {
            OwnerType = ownerType,
            OwnerId = ownerId
        };
        var result = await sender.Send(query);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetChannelsByType(
        OwnerType ownerType,
        string ownerId,
        ChannelType channelType,
        ISender sender)
    {
        var query = new GetChannelsByTypeQuery
        {
            OwnerType = ownerType,
            OwnerId = ownerId,
            ChannelType = channelType
        };
        var result = await sender.Send(query);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetPrimaryChannels(
        OwnerType ownerType,
        string ownerId,
        ISender sender)
    {
        var query = new GetPrimaryChannelsQuery
        {
            OwnerType = ownerType,
            OwnerId = ownerId
        };
        var result = await sender.Send(query);
        return Results.Ok(result);
    }

    private static async Task<IResult> CreateChannel(
        CreateChannelIdentityCommand command,
        ISender sender)
    {
        var result = await sender.Send(command);
        return Results.Created($"/api/crm/channels/{result.Id}", result);
    }

    private static async Task<IResult> UpdateChannel(
        UpdateChannelIdentityCommand command,
        ISender sender)
    {
        await sender.Send(command);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteChannel(
        string id,
        ISender sender)
    {
        await sender.Send(new DeleteChannelIdentityCommand { Id = id });
        return Results.NoContent();
    }

    private static async Task<IResult> VerifyChannel(
        string id,
        ISender sender)
    {
        await sender.Send(new VerifyChannelCommand { Id = id });
        return Results.NoContent();
    }

    private static async Task<IResult> UpdateOptIn(
        string id,
        [FromBody] UpdateOptInCommand command,
        ISender sender)
    {
        // Ensure the ID from the route matches the command
        if (command.Id != id)
        {
            return Results.BadRequest("ID mismatch");
        }

        await sender.Send(command);
        return Results.NoContent();
    }
}
