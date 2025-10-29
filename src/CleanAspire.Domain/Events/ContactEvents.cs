using CleanAspire.Domain.Entities;

namespace CleanAspire.Domain.Events;

/// <summary>
/// Domain event triggered when a new contact is created.
/// </summary>
public class ContactCreatedEvent : DomainEvent
{
    public Contact Contact { get; }

    public ContactCreatedEvent(Contact contact)
    {
        Contact = contact;
    }
}

/// <summary>
/// Domain event triggered when a contact is updated.
/// </summary>
public class ContactUpdatedEvent : DomainEvent
{
    public Contact Contact { get; }

    public ContactUpdatedEvent(Contact contact)
    {
        Contact = contact;
    }
}

/// <summary>
/// Domain event triggered when a contact is deleted.
/// </summary>
public class ContactDeletedEvent : DomainEvent
{
    public Contact Contact { get; }

    public ContactDeletedEvent(Contact contact)
    {
        Contact = contact;
    }
}