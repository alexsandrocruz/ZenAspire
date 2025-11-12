using CleanAspire.Application.Pipeline;
using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Events;

namespace CleanAspire.Application.Features.Clients.Commands;

/// <summary>
/// Command for updating an existing client.
/// Contains all updatable client properties.
/// </summary>
public record UpdateClientCommand : IFusionCacheRefreshRequest<Unit>, IRequiresValidation
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? TradeName { get; init; }
    public string? DocumentNumber { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Website { get; init; }
    
    // Address
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }
    
    // Business Information
    public string? Industry { get; init; }
    public string? Size { get; init; }
    public decimal? AnnualRevenue { get; init; }
    public int? EmployeeCount { get; init; }
    
    // Relationship Information
    public ClientType Type { get; init; }
    public ClientStatus Status { get; init; }
    public ClientPriority Priority { get; init; }
    
    public string? Notes { get; init; }
    public string? ClientTags { get; init; }
    
    // Important Dates
    public DateTime? FirstContactDate { get; init; }
    public DateTime? LastInteractionDate { get; init; }
    
    public IEnumerable<string>? Tags => new[] { "clients" };
}

/// <summary>
/// Handler for processing UpdateClientCommand.
/// Updates an existing client entity in the database.
/// </summary>
public class UpdateClientCommandHandler : IRequestHandler<UpdateClientCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public UpdateClientCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Unit> Handle(UpdateClientCommand request, CancellationToken cancellationToken)
    {
        var client = await _context.Clients
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (client == null)
        {
            throw new KeyNotFoundException($"Client with Id '{request.Id}' was not found.");
        }

        client.Name = request.Name;
        client.TradeName = request.TradeName;
        client.DocumentNumber = request.DocumentNumber;
        client.Email = request.Email;
        client.Phone = request.Phone;
        client.Website = request.Website;
        client.Address = request.Address;
        client.City = request.City;
        client.State = request.State;
        client.PostalCode = request.PostalCode;
        client.Country = request.Country;
        client.Industry = request.Industry;
        client.Size = request.Size;
        client.AnnualRevenue = request.AnnualRevenue;
        client.EmployeeCount = request.EmployeeCount;
        client.Type = request.Type;
        client.Status = request.Status;
        client.Priority = request.Priority;
        client.Notes = request.Notes;
        client.Tags = request.ClientTags;
        client.FirstContactDate = request.FirstContactDate;
        client.LastInteractionDate = request.LastInteractionDate ?? DateTime.UtcNow;

        // Add domain event for auditing purposes
        client.AddDomainEvent(new ClientUpdatedEvent(client));

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}