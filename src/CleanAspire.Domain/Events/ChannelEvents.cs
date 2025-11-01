// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Domain.Common;
using CleanAspire.Domain.Entities;

namespace CleanAspire.Domain.Events;

/// <summary>
/// Event fired when a channel identity is created
/// </summary>
public class ChannelIdentityCreatedEvent : DomainEvent
{
    public ChannelIdentity Item { get; }
    public ChannelIdentityCreatedEvent(ChannelIdentity item)
    {
        Item = item;
    }
}

/// <summary>
/// Event fired when a channel identity is updated
/// </summary>
public class ChannelIdentityUpdatedEvent : DomainEvent
{
    public ChannelIdentity Item { get; }
    public ChannelIdentityUpdatedEvent(ChannelIdentity item)
    {
        Item = item;
    }
}

/// <summary>
/// Event fired when a channel identity is deleted
/// </summary>
public class ChannelIdentityDeletedEvent : DomainEvent
{
    public ChannelIdentity Item { get; }
    public ChannelIdentityDeletedEvent(ChannelIdentity item)
    {
        Item = item;
    }
}

/// <summary>
/// Event fired when a channel identity is verified
/// </summary>
public class ChannelIdentityVerifiedEvent : DomainEvent
{
    public ChannelIdentity Item { get; }
    public ChannelIdentityVerifiedEvent(ChannelIdentity item)
    {
        Item = item;
    }
}

/// <summary>
/// Event fired when opt-in status changes
/// </summary>
public class ChannelOptInChangedEvent : DomainEvent
{
    public ChannelIdentity Item { get; }
    public bool WasOptedIn { get; }
    public ChannelOptInChangedEvent(ChannelIdentity item, bool wasOptedIn)
    {
        Item = item;
        WasOptedIn = wasOptedIn;
    }
}
