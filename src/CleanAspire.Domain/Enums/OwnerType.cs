// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace CleanAspire.Domain.Enums;

/// <summary>
/// Defines the type of entity that can own tags, notes, attachments, etc.
/// Used for polymorphic associations.
/// </summary>
public enum OwnerType
{
    /// <summary>
    /// Client/Account entity
    /// </summary>
    Client = 1,

    /// <summary>
    /// Contact entity
    /// </summary>
    Contact = 2,

    /// <summary>
    /// Activity entity (future)
    /// </summary>
    Activity = 3,

    /// <summary>
    /// Opportunity entity (future)
    /// </summary>
    Opportunity = 4,

    /// <summary>
    /// Deal entity (future)
    /// </summary>
    Deal = 5,

    /// <summary>
    /// Case/Ticket entity (future)
    /// </summary>
    Case = 6
}
