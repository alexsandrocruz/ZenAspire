using CleanAspire.Application.Pipeline;
using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Events;

namespace CleanAspire.Application.Features.Contacts.Commands;

/// <summary>
/// Command for deleting a contact by ID.
/// Includes validation to ensure contact exists.
/// </summary>
public record DeleteContactCommand(string Id) : IFusionCacheRefreshRequest<Unit>, IRequiresValidation
{
    public IEnumerable<string>? Tags => new[] { "contacts" };
}

/// <summary>
/// Handler for processing DeleteContactCommand.
/// Deletes the specified contact from the database.
/// </summary>
public class DeleteContactCommandHandler : IRequestHandler<DeleteContactCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteContactCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Unit> Handle(DeleteContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await _context.Contacts
            .Include(x => x.Client)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (contact == null)
        {
            throw new KeyNotFoundException($"Contact with Id '{request.Id}' was not found.");
        }

        // Add domain event before deletion for auditing purposes
        contact.AddDomainEvent(new ContactDeletedEvent(contact));

        // Update client's last interaction date
        if (contact.Client != null)
        {
            contact.Client.LastInteractionDate = DateTime.UtcNow;
        }

        // Remove contact
        _context.Contacts.Remove(contact);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}