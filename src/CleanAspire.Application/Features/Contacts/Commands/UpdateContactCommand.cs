using CleanAspire.Application.Pipeline;
using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Events;

namespace CleanAspire.Application.Features.Contacts.Commands;

/// <summary>
/// Command for updating an existing contact.
/// Contains all updatable contact properties.
/// </summary>
public record UpdateContactCommand : IFusionCacheRefreshRequest<Unit>, IRequiresValidation
{
    public string Id { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? MobilePhone { get; init; }
    public string? JobTitle { get; init; }
    public string? Department { get; init; }
    
    // Personal Address (optional)
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? PostalCode { get; init; }
    
    public string? Notes { get; init; }
    public string? ContactTags { get; init; }
    
    // Relationship Information
    public ContactType Type { get; init; }
    public ContactStatus Status { get; init; }
    
    public bool IsMainContact { get; init; }
    public bool IsDecisionMaker { get; init; }
    
    // Important Dates
    public DateTime? BirthDate { get; init; }
    public DateTime? LastContactDate { get; init; }
    
    // Client Information (cannot be changed after creation)
    public string ClientId { get; init; } = string.Empty;
    
    public IEnumerable<string>? Tags => new[] { "contacts" };
}

/// <summary>
/// Handler for processing UpdateContactCommand.
/// Updates an existing contact entity in the database.
/// </summary>
public class UpdateContactCommandHandler : IRequestHandler<UpdateContactCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public UpdateContactCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Unit> Handle(UpdateContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await _context.Contacts
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (contact == null)
        {
            throw new KeyNotFoundException($"Contact with Id '{request.Id}' was not found.");
        }

        // Verify that the client exists (if client is being changed)
        if (contact.ClientId.ToString() != request.ClientId)
        {
            var clientExists = await _context.Clients
                .AnyAsync(c => c.Id == request.ClientId, cancellationToken);

            if (!clientExists)
            {
                throw new KeyNotFoundException($"Client with Id '{request.ClientId}' was not found.");
            }

            contact.ClientId = request.ClientId;
        }

        contact.FirstName = request.FirstName;
        contact.LastName = request.LastName;
        contact.Email = request.Email;
        contact.Phone = request.Phone;
        contact.MobilePhone = request.MobilePhone;
        contact.JobTitle = request.JobTitle;
        contact.Department = request.Department;
        contact.Address = request.Address;
        contact.City = request.City;
        contact.State = request.State;
        contact.PostalCode = request.PostalCode;
        contact.Notes = request.Notes;
        contact.Tags = request.ContactTags;
        contact.Type = request.Type;
        contact.Status = request.Status;
        contact.IsMainContact = request.IsMainContact;
        contact.IsDecisionMaker = request.IsDecisionMaker;
        contact.BirthDate = request.BirthDate;
        contact.LastContactDate = request.LastContactDate ?? DateTime.UtcNow;

        // Add domain event for auditing purposes
        contact.AddDomainEvent(new ContactUpdatedEvent(contact));

        // Update client's last interaction date
        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.Id == contact.ClientId, cancellationToken);
        if (client != null)
        {
            client.LastInteractionDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}