// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Domain.Common;
using CleanAspire.Domain.Entities;

namespace CleanAspire.Domain.Events;

/// <summary>
/// Event fired when an address is created
/// </summary>
public class AddressCreatedEvent : DomainEvent
{
    public Address Item { get; }
    public AddressCreatedEvent(Address item)
    {
        Item = item;
    }
}

/// <summary>
/// Event fired when an address is updated
/// </summary>
public class AddressUpdatedEvent : DomainEvent
{
    public Address Item { get; }
    public AddressUpdatedEvent(Address item)
    {
        Item = item;
    }
}

/// <summary>
/// Event fired when an address is deleted
/// </summary>
public class AddressDeletedEvent : DomainEvent
{
    public Address Item { get; }
    public AddressDeletedEvent(Address item)
    {
        Item = item;
    }
}

/// <summary>
/// Event fired when an address is set as primary
/// </summary>
public class AddressPrimaryChangedEvent : DomainEvent
{
    public Address Item { get; }
    public bool WasPrimary { get; }
    public AddressPrimaryChangedEvent(Address item, bool wasPrimary)
    {
        Item = item;
        WasPrimary = wasPrimary;
    }
}
