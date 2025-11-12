using CleanAspire.Application.Pipeline;
using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Events;

namespace CleanAspire.Application.Features.Clients.Commands;

/// <summary>
/// Command for deleting a client by ID.
/// Includes validation to ensure client exists.
/// </summary>
public record DeleteClientCommand(string Id) : IFusionCacheRefreshRequest<Unit>, IRequiresValidation
{
    public IEnumerable<string>? Tags => new[] { "clients" };
}

/// <summary>
/// Handler for processing DeleteClientCommand.
/// Deletes the specified client and all related contacts from the database.
/// </summary>
public class DeleteClientCommandHandler : IRequestHandler<DeleteClientCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteClientCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Unit> Handle(DeleteClientCommand request, CancellationToken cancellationToken)
    {
        var client = await _context.Clients
            .Include(x => x.Contacts)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (client == null)
        {
            throw new KeyNotFoundException($"Client with Id '{request.Id}' was not found.");
        }

        // Add domain event before deletion for auditing purposes
        client.AddDomainEvent(new ClientDeletedEvent(client));

        // Remove client (contacts will be cascade deleted due to FK configuration)
        _context.Clients.Remove(client);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}