// Summary:
// This file contains the endpoint registration for Customer-related API operations.
// It sets up RESTful endpoints for CRUD operations including Create, Read, Update, Delete,
// as well as specialized endpoints for Import/Export functionality.

using CleanAspire.Application.Features.Customers.Commands;
using CleanAspire.Application.Features.Customers.DTOs;
using CleanAspire.Application.Features.Customers.Queries;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace CleanAspire.Api.Endpoints;

/// <summary>
/// Registers all Customer-related API endpoints with the web application.
/// Implements IEndpointRegistrar to follow the modular endpoint registration pattern.
/// </summary>
public class CustomerEndpointRegistrar : IEndpointRegistrar
{
    /// <summary>
    /// Registers all Customer endpoints with the provided IEndpointRouteBuilder.
    /// </summary>
    /// <param name="routes">The IEndpointRouteBuilder to register endpoints with.</param>
    public void RegisterRoutes(IEndpointRouteBuilder routes)
    {
        // Group all customer endpoints under /customers route
        var customerGroup = routes.MapGroup("/customers").WithTags("Customers");

        // GET /customers - Retrieve all customers with pagination and filtering
        customerGroup.MapGet("/", async (
            ISender sender,
            [AsParameters] CustomersWithPaginationQuery query) =>
        {
            var result = await sender.Send(query);
            return Results.Ok(result);
        })
        .WithName("GetCustomers")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Get paginated customers";
            operation.Description = "Retrieves a paginated list of customers with optional filtering and sorting.";
            return operation;
        });

        // GET /customers/{id} - Retrieve a specific customer by ID
        customerGroup.MapGet("/{id}", async (
            ISender sender,
            string id) =>
        {
            var result = await sender.Send(new GetCustomerByIdQuery(id));
            return result != null ? Results.Ok(result) : Results.NotFound();
        })
        .WithName("GetCustomerById")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Get customer by ID";
            operation.Description = "Retrieves a specific customer by their unique identifier.";
            return operation;
        });

        // POST /customers - Create a new customer
        customerGroup.MapPost("/", async (
            ISender sender,
            CreateCustomerCommand command) =>
        {
            var result = await sender.Send(command);
            return Results.Created($"/customers/{result.Id}", result);
        })
        .WithName("CreateCustomer")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Create a new customer";
            operation.Description = "Creates a new customer with the provided information.";
            return operation;
        });

        // PUT /customers/{id} - Update an existing customer
        customerGroup.MapPut("/{id}", async (
            ISender sender,
            string id,
            UpdateCustomerCommand command) =>
        {
            if (id != command.Id)
                return Results.BadRequest("ID mismatch between route and body.");

            var result = await sender.Send(command);
            return result != null ? Results.Ok(result) : Results.NotFound();
        })
        .WithName("UpdateCustomer")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Update an existing customer";
            operation.Description = "Updates an existing customer with the provided information.";
            return operation;
        });

        // DELETE /customers/{id} - Delete a customer
        customerGroup.MapDelete("/{id}", async (
            ISender sender,
            string id) =>
        {
            var result = await sender.Send(new DeleteCustomerCommand(id));
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteCustomer")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Delete a customer";
            operation.Description = "Deletes a customer by their unique identifier.";
            return operation;
        });

        // GET /customers/export - Export customers to CSV
        customerGroup.MapGet("/export", async (
            ISender sender,
            [FromQuery] string keywords = "") =>
        {
            var result = await sender.Send(new ExportCustomersQuery(keywords));
            result.Position = 0;
            return Results.File(result, "text/csv", "customers.csv");
        })
        .WithName("ExportCustomers")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Export customers to CSV";
            operation.Description = "Exports filtered customers to a CSV file for download.";
            return operation;
        });

        // POST /customers/import - Import customers from CSV
        customerGroup.MapPost("/import", async (
            ISender sender,
            IFormFile file) =>
        {
            if (file == null || file.Length == 0)
                return Results.BadRequest("No file uploaded.");

            if (!file.ContentType.Equals("text/csv", StringComparison.OrdinalIgnoreCase))
                return Results.BadRequest("File must be a CSV.");

            using var stream = file.OpenReadStream();
            await sender.Send(new ImportCustomersCommand(stream));
            return Results.Ok(new { Message = "Customers imported successfully." });
        })
        .WithName("ImportCustomers")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Import customers from CSV";
            operation.Description = "Imports customers from an uploaded CSV file.";
            return operation;
        })
        .DisableAntiforgery(); // Disable antiforgery for file uploads
    }
}