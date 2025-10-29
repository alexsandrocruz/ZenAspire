using CleanAspire.Application.Features.Clients.DTOs;
using CleanAspire.Application.Pipeline;
using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Events;

namespace CleanAspire.Application.Features.Clients.Commands;

/// <summary>
/// Command for creating a new client.
/// Encapsulates all data needed to create a client entity.
/// </summary>
public record CreateClientCommand : IFusionCacheRefreshRequest<ClientDto>, IRequiresValidation
{
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
    public ClientType Type { get; init; } = ClientType.Company;
    public ClientStatus Status { get; init; } = ClientStatus.Prospect;
    public ClientPriority Priority { get; init; } = ClientPriority.Medium;
    
    public string? Notes { get; init; }
    public string? ClientTags { get; init; }
    
    // Important Dates
    public DateTime? FirstContactDate { get; init; }
    
    public IEnumerable<string>? Tags => new[] { "clients" };
}

/// <summary>
/// Handler for processing CreateClientCommand.
/// Creates a new client entity and saves it to the database.
/// </summary>
public class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, ClientDto>
{
    private readonly IApplicationDbContext _context;

    public CreateClientCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<ClientDto> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        var client = new Client
        {
            Name = request.Name,
            TradeName = request.TradeName,
            DocumentNumber = request.DocumentNumber,
            Email = request.Email,
            Phone = request.Phone,
            Website = request.Website,
            Address = request.Address,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            Country = request.Country,
            Industry = request.Industry,
            Size = request.Size,
            AnnualRevenue = request.AnnualRevenue,
            EmployeeCount = request.EmployeeCount,
            Type = request.Type,
            Status = request.Status,
            Priority = request.Priority,
            Notes = request.Notes,
            Tags = request.ClientTags,
            FirstContactDate = request.FirstContactDate ?? DateTime.UtcNow,
            LastInteractionDate = DateTime.UtcNow
        };

        // Add domain event for auditing/notification purposes
        client.AddDomainEvent(new ClientCreatedEvent(client));
        
        _context.Clients.Add(client);
        await _context.SaveChangesAsync(cancellationToken);

        return new ClientDto
        {
            Id = client.Id.ToString(),
            Name = client.Name,
            TradeName = client.TradeName,
            Email = client.Email,
            Type = (int)client.Type,
            TypeName = client.Type.ToString(),
            Status = (int)client.Status,
            StatusName = client.Status.ToString(),
            Priority = (int)client.Priority,
            PriorityName = client.Priority.ToString(),
            Created = client.Created,
            CreatedBy = client.CreatedBy
        };
    }
}