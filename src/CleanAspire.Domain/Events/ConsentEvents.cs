using CleanAspire.Domain.Common;
using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Enums;

namespace CleanAspire.Domain.Events;

/// <summary>
/// Event raised when a consent is created or updated.
/// Phase 4: LGPD Compliance - Consent & Data Privacy
/// </summary>
public class ConsentChangedEvent : DomainEvent
{
    public OwnerType OwnerType { get; }
    public Guid OwnerId { get; }
    public ConsentPurpose Purpose { get; }
    public bool OptIn { get; }
    public DateTime At { get; }
    public string Channel { get; }
    public string? Source { get; }

    public ConsentChangedEvent(Consent consent)
    {
        OwnerType = consent.OwnerType;
        OwnerId = consent.OwnerId;
        Purpose = consent.Purpose;
        OptIn = consent.OptIn;
        At = consent.At;
        Channel = consent.Channel;
        Source = consent.Source;
    }
}

/// <summary>
/// Event raised when a new consent is recorded.
/// </summary>
public class ConsentCreatedEvent : ConsentChangedEvent
{
    public Consent Item { get; }

    public ConsentCreatedEvent(Consent consent) : base(consent)
    {
        Item = consent;
    }
}

/// <summary>
/// Event raised when an existing consent is updated.
/// </summary>
public class ConsentUpdatedEvent : ConsentChangedEvent
{
    public Consent Item { get; }

    public ConsentUpdatedEvent(Consent consent) : base(consent)
    {
        Item = consent;
    }
}

/// <summary>
/// Event raised when a consent is revoked/withdrawn.
/// </summary>
public class ConsentRevokedEvent : ConsentChangedEvent
{
    public Consent Item { get; }

    public ConsentRevokedEvent(Consent consent) : base(consent)
    {
        Item = consent;
    }
}

/// <summary>
/// Event raised when data erasure is requested for a subject.
/// Phase 4: LGPD Compliance - Data Erasure Workflow
/// </summary>
public class SubjectErasureRequestedEvent : DomainEvent
{
    public OwnerType OwnerType { get; }
    public Guid OwnerId { get; }
    public string RequestedBy { get; }
    public string? Reason { get; }
    public DateTime RequestedAt { get; }

    public SubjectErasureRequestedEvent(
        OwnerType ownerType,
        Guid ownerId,
        string requestedBy,
        string? reason = null)
    {
        OwnerType = ownerType;
        OwnerId = ownerId;
        RequestedBy = requestedBy;
        Reason = reason;
        RequestedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Event raised when personal data export is requested.
/// Phase 4: LGPD Compliance - Data Export Workflow
/// </summary>
public class PersonalDataExportRequestedEvent : DomainEvent
{
    public OwnerType OwnerType { get; }
    public Guid OwnerId { get; }
    public string RequestedBy { get; }
    public string Format { get; } // "JSON" or "CSV"
    public DateTime RequestedAt { get; }

    public PersonalDataExportRequestedEvent(
        OwnerType ownerType,
        Guid ownerId,
        string requestedBy,
        string format = "JSON")
    {
        OwnerType = ownerType;
        OwnerId = ownerId;
        RequestedBy = requestedBy;
        Format = format;
        RequestedAt = DateTime.UtcNow;
    }
}