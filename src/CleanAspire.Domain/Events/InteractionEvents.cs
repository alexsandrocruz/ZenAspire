// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Domain.Entities;

namespace CleanAspire.Domain.Events;

/// <summary>
/// Domain event raised when a new interaction is captured in the system.
/// </summary>
public class InteractionCapturedEvent : DomainEvent
{
    public Interaction Item { get; }

    public InteractionCapturedEvent(Interaction item)
    {
        Item = item;
    }
}

/// <summary>
/// Domain event raised when an interaction is updated.
/// </summary>
public class InteractionUpdatedEvent : DomainEvent
{
    public Interaction Item { get; }

    public InteractionUpdatedEvent(Interaction item)
    {
        Item = item;
    }
}

/// <summary>
/// Domain event raised when an interaction is deleted.
/// </summary>
public class InteractionDeletedEvent : DomainEvent
{
    public Interaction Item { get; }

    public InteractionDeletedEvent(Interaction item)
    {
        Item = item;
    }
}
