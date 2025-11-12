using CleanAspire.Application.Features.Contacts.DTOs;
using CleanAspire.Application.Pipeline;
using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Events;

namespace CleanAspire.Application.Features.Contacts.Commands;

/// <summary>
/// Command for creating a new contact.
/// Encapsulates all data needed to create a contact entity.
/// </summary>
public record CreateContactCommand : IFusionCacheRefreshRequest<ContactDto>, IRequiresValidation
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? Mobile { get; init; } // ✅ Replaces MobilePhone
    [Obsolete("Use Mobile instead")]
    public string? MobilePhone
    {
        get => Mobile;
        init => Mobile = value;
    }
    public string? JobTitle { get; init; }
    public string? Department { get; init; }

    // Personal Address (optional)
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? PostalCode { get; init; }

    public string? Notes { get; init; }
    public string? ContactTags { get; init; }

    // ✅ CRM lifecycle
    public string LifecycleStage { get; init; } = "Lead";
    public string? OwnerUserId { get; init; }

    // Relationship Information
    public ContactType Type { get; init; } = ContactType.Lead;
    public ContactStatus Status { get; init; } = ContactStatus.Active;

    public bool IsMainContact { get; init; } = false;
    public bool IsDecisionMaker { get; init; } = false;

    // Important Dates
    public DateTime? BirthDate { get; init; }

    // Client Information
    public string ClientId { get; init; } = string.Empty;

    public IEnumerable<string>? Tags => new[] { "contacts" };
}

/// <summary>
/// Handler for processing CreateContactCommand.
/// Creates a new contact entity and saves it to the database.
/// </summary>
public class CreateContactCommandHandler : IRequestHandler<CreateContactCommand, ContactDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser; // ✅ Inject current user service

    public CreateContactCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<ContactDto> Handle(CreateContactCommand request, CancellationToken cancellationToken)
    {
        // ✅ Validate tenant
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            throw new UnauthorizedAccessException("TenantId is required");

        // Verify that the client exists
        if (string.IsNullOrWhiteSpace(request.ClientId))
        {
            throw new ArgumentException("Client ID cannot be null or empty.", nameof(request.ClientId));
        }

        // ✅ Client lookup will be automatically filtered by TenantId (global query filter)
        var clientExists = await _context.Clients
            .AnyAsync(c => c.Id == request.ClientId, cancellationToken);

        if (!clientExists)
        {
            throw new KeyNotFoundException($"Client with Id '{request.ClientId}' was not found.");
        }

        var contact = new Contact
        {
            TenantId = _currentUser.TenantId, // ✅ Set TenantId from current user
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Mobile = request.Mobile, // ✅ Use Mobile instead of MobilePhone
            JobTitle = request.JobTitle,
            Department = request.Department,
            Address = request.Address,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            Notes = request.Notes,
            Tags = request.ContactTags,
            LifecycleStage = request.LifecycleStage, // ✅ New field
            OwnerUserId = request.OwnerUserId ?? _currentUser.UserId, // ✅ Default to current user
            Type = request.Type,
            Status = request.Status,
            IsMainContact = request.IsMainContact,
            IsDecisionMaker = request.IsDecisionMaker,
            BirthDate = request.BirthDate,
            LastContactDate = DateTime.UtcNow,
            ClientId = request.ClientId
        };

        // Add domain event for auditing/notification purposes
        contact.AddDomainEvent(new ContactCreatedEvent(contact));
        
        _context.Contacts.Add(contact);
        await _context.SaveChangesAsync(cancellationToken);

        // Update client's last interaction date
        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.Id == request.ClientId, cancellationToken);
        if (client != null)
        {
            client.LastInteractionDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new ContactDto
        {
            Id = contact.Id,
            FirstName = contact.FirstName,
            LastName = contact.LastName,
            Email = contact.Email,
            Phone = contact.Phone,
            JobTitle = contact.JobTitle,
            Type = (int)contact.Type,
            TypeName = contact.Type.ToString(),
            Status = (int)contact.Status,
            StatusName = contact.Status.ToString(),
            IsMainContact = contact.IsMainContact,
            IsDecisionMaker = contact.IsDecisionMaker,
            ClientId = contact.ClientId,
            ClientName = client?.Name ?? string.Empty,
            ClientDisplayName = client?.DisplayName ?? string.Empty,
            Created = contact.Created,
            CreatedBy = contact.CreatedBy
        };
    }
}