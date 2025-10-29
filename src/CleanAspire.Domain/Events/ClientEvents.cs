using CleanAspire.Domain.Entities;

namespace CleanAspire.Domain.Events;

/// <summary>
/// Domain event triggered when a new client is created.
/// </summary>
public class ClientCreatedEvent : DomainEvent
{
    public Client Client { get; }

    public ClientCreatedEvent(Client client)
    {
        Client = client;
    }
}

/// <summary>
/// Domain event triggered when a client is updated.
/// </summary>
public class ClientUpdatedEvent : DomainEvent
{
    public Client Client { get; }

    public ClientUpdatedEvent(Client client)
    {
        Client = client;
    }
}

/// <summary>
/// Domain event triggered when a client is deleted.
/// </summary>
public class ClientDeletedEvent : DomainEvent
{
    public Client Client { get; }

    public ClientDeletedEvent(Client client)
    {
        Client = client;
    }
}