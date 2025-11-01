using CleanAspire.Domain.Common;
using CleanAspire.Domain.Entities;

namespace CleanAspire.Domain.Events;

// ✅ Follow CleanAspire pattern: pass full entity
public class ContactCreatedEvent : DomainEvent
{
    public Contact Item { get; }
    public ContactCreatedEvent(Contact item)
    {
        Item = item;
    }
}

public class ContactUpdatedEvent : DomainEvent
{
    public Contact Item { get; }
    public ContactUpdatedEvent(Contact item)
    {
        Item = item;
    }
}

public class ContactDeletedEvent : DomainEvent
{
    public Contact Item { get; }
    public ContactDeletedEvent(Contact item)
    {
        Item = item;
    }
}