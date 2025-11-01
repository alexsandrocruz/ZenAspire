using CleanAspire.Domain.Common;
using CleanAspire.Domain.Entities;

namespace CleanAspire.Domain.Events;

// ✅ Follow CleanAspire pattern: pass full entity
public class ClientCreatedEvent : DomainEvent
{
    public Client Item { get; }
    public ClientCreatedEvent(Client item)
    {
        Item = item;
    }
}

public class ClientUpdatedEvent : DomainEvent
{
    public Client Item { get; }
    public ClientUpdatedEvent(Client item)
    {
        Item = item;
    }
}

public class ClientDeletedEvent : DomainEvent
{
    public Client Item { get; }
    public ClientDeletedEvent(Client item)
    {
        Item = item;
    }
}