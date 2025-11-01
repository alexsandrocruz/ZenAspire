// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Features.Addresses.Commands;
using CleanAspire.Application.Features.Addresses.DTOs;
using CleanAspire.Application.Features.Addresses.Queries;
using CleanAspire.Domain.Enums;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace CleanAspire.Api.Endpoints;

/// <summary>
/// API endpoints for Address management
/// </summary>
public class AddressEndpointRegistrar : IEndpointRegistrar
{
    public void RegisterRoutes(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/crm/addresses")
            .WithTags("CRM.Addresses");
            // .RequireAuthorization(); // TODO: Enable after testing

        // GET /api/crm/addresses/{id}
        group.MapGet("/{id}", GetAddressById)
            .WithName("GetAddressById")
            .WithSummary("Get address by ID")
            .Produces<AddressDto>()
            .Produces(404);

        // GET /api/crm/addresses/owner/{ownerType}/{ownerId}
        group.MapGet("/owner/{ownerType}/{ownerId}", GetAddressesByOwner)
            .WithName("GetAddressesByOwner")
            .WithSummary("Get all addresses for an owner (Client or Contact)")
            .Produces<List<AddressDto>>();

        // GET /api/crm/addresses/owner/{ownerType}/{ownerId}/primary
        group.MapGet("/owner/{ownerType}/{ownerId}/primary", GetPrimaryAddress)
            .WithName("GetPrimaryAddress")
            .WithSummary("Get the primary address for an owner")
            .Produces<AddressDto>()
            .Produces(404);

        // POST /api/crm/addresses
        group.MapPost("/", CreateAddress)
            .WithName("CreateAddress")
            .WithSummary("Create a new address")
            .Produces<AddressDto>(201)
            .ProducesValidationProblem();

        // PUT /api/crm/addresses
        group.MapPut("/", UpdateAddress)
            .WithName("UpdateAddress")
            .WithSummary("Update an existing address")
            .Produces(204)
            .Produces(404)
            .ProducesValidationProblem();

        // DELETE /api/crm/addresses/{id}
        group.MapDelete("/{id}", DeleteAddress)
            .WithName("DeleteAddress")
            .WithSummary("Delete an address")
            .Produces(204)
            .Produces(404);

        // POST /api/crm/addresses/{id}/set-primary
        group.MapPost("/{id}/set-primary", SetPrimaryAddress)
            .WithName("SetPrimaryAddress")
            .WithSummary("Set an address as primary for its owner")
            .Produces(204)
            .Produces(404);
    }

    private static async Task<IResult> GetAddressById(
        string id,
        ISender sender)
    {
        var query = new GetAddressByIdQuery(id);
        var result = await sender.Send(query);
        return result is not null ? Results.Ok(result) : Results.NotFound();
    }

    private static async Task<IResult> GetAddressesByOwner(
        OwnerType ownerType,
        string ownerId,
        ISender sender)
    {
        var query = new GetAddressesByOwnerQuery
        {
            OwnerType = ownerType,
            OwnerId = ownerId
        };
        var result = await sender.Send(query);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetPrimaryAddress(
        OwnerType ownerType,
        string ownerId,
        ISender sender)
    {
        var query = new GetPrimaryAddressQuery
        {
            OwnerType = ownerType,
            OwnerId = ownerId
        };
        var result = await sender.Send(query);
        return result is not null ? Results.Ok(result) : Results.NotFound();
    }

    private static async Task<IResult> CreateAddress(
        CreateAddressCommand command,
        ISender sender)
    {
        var result = await sender.Send(command);
        return Results.Created($"/api/crm/addresses/{result.Id}", result);
    }

    private static async Task<IResult> UpdateAddress(
        UpdateAddressCommand command,
        ISender sender)
    {
        await sender.Send(command);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteAddress(
        string id,
        ISender sender)
    {
        await sender.Send(new DeleteAddressCommand { Id = id });
        return Results.NoContent();
    }

    private static async Task<IResult> SetPrimaryAddress(
        string id,
        ISender sender)
    {
        await sender.Send(new SetPrimaryAddressCommand { Id = id });
        return Results.NoContent();
    }
}
