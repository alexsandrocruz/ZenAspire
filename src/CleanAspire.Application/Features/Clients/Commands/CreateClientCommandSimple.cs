using CleanAspire.Application.Features.Clients.DTOs;
using CleanAspire.Application.Pipeline;
using CleanAspire.Domain.Entities;

namespace CleanAspire.Application.Features.Clients.Commands;

/// <summary>
/// Simplified command for creating a new client.
/// </summary>
public record CreateClientCommandSimple : IFusionCacheRefreshRequest<ClientDto>, IRequiresValidation
{
    public string Name { get; init; } = string.Empty;
    public string? Email { get; init; }
    public ClientType Type { get; init; } = ClientType.Company;
    public ClientStatus Status { get; init; } = ClientStatus.Prospect;
    
    public IEnumerable<string>? Tags => new[] { "clients" };
}

/// <summary>
/// Simple handler for creating clients.
/// </summary>
public class CreateClientSimpleCommandHandler : IRequestHandler<CreateClientCommandSimple, ClientDto>
{
    private readonly IApplicationDbContext _context;

    public CreateClientSimpleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<ClientDto> Handle(CreateClientCommandSimple request, CancellationToken cancellationToken)
    {
        var client = new Client
        {
            Name = request.Name,
            Email = request.Email,
            Type = request.Type,
            Status = request.Status,
            Priority = ClientPriority.Medium,
            FirstContactDate = DateTime.UtcNow,
            LastInteractionDate = DateTime.UtcNow
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync(cancellationToken);

        return new ClientDto
        {
            Id = client.Id.ToString(),
            Name = client.Name,
            Email = client.Email,
            Type = (int)client.Type,
            TypeName = client.Type.ToString(),
            Status = (int)client.Status,
            StatusName = client.Status.ToString(),
            Priority = (int)client.Priority,
            PriorityName = client.Priority.ToString()
        };
    }
}