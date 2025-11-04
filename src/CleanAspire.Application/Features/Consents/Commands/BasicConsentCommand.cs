using CleanAspire.Application.Features.Consents.DTOs;
using CleanAspire.Application.Pipeline;
using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Enums;
using CleanAspire.Domain.Events;

namespace CleanAspire.Application.Features.Consents.Commands;

/// <summary>
/// Basic Command for recording a new consent.
/// Phase 4: LGPD Compliance - Consent & Data Privacy
/// </summary>
public record BasicConsentCommand : IFusionCacheRefreshRequest<ConsentDto>, IRequiresValidation
{
    public int OwnerType { get; init; }
    public Guid OwnerId { get; init; }
    public int Purpose { get; init; }
    public bool OptIn { get; init; }
    public string Channel { get; init; } = string.Empty;
    public DateTime? At { get; init; }
    public string? Source { get; init; }
    public string? Notes { get; init; }
    public DateTime? ValidUntil { get; init; }

    public IEnumerable<string>? Tags => new[] { "consents" };
}

/// <summary>
/// Basic Handler for processing BasicConsentCommand.
/// Records a new consent entity and raises appropriate domain events.
/// </summary>
public class BasicConsentCommandHandler : IRequestHandler<BasicConsentCommand, ConsentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public BasicConsentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<ConsentDto> Handle(BasicConsentCommand request, CancellationToken cancellationToken)
    {
        // Validate tenant
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            throw new UnauthorizedAccessException("TenantId is required");

        // Validate enum values
        if (!Enum.IsDefined(typeof(OwnerType), request.OwnerType))
            throw new ArgumentException("Invalid OwnerType value");

        if (!Enum.IsDefined(typeof(ConsentPurpose), request.Purpose))
            throw new ArgumentException("Invalid ConsentPurpose value");

        // Check if consent already exists for this owner, purpose, and tenant
        var existingConsent = await _context.Consents
            .FirstOrDefaultAsync(c => c.TenantId == _currentUser.TenantId &&
                                     c.OwnerType == (OwnerType)request.OwnerType &&
                                     c.OwnerId == request.OwnerId &&
                                     c.Purpose == (ConsentPurpose)request.Purpose,
                cancellationToken);

        Consent consent;

        if (existingConsent != null)
        {
            // Update existing consent
            existingConsent.OptIn = request.OptIn;
            existingConsent.Channel = request.Channel;
            existingConsent.At = request.At ?? DateTime.UtcNow;
            existingConsent.Source = request.Source;
            existingConsent.Notes = request.Notes;
            existingConsent.ValidUntil = request.ValidUntil;
            existingConsent.LastModified = DateTime.UtcNow;
            existingConsent.LastModifiedBy = _currentUser.UserId;

            // Add domain event for consent update
            existingConsent.AddDomainEvent(new ConsentUpdatedEvent(existingConsent));

            consent = existingConsent;
        }
        else
        {
            // Create new consent
            consent = new Consent
            {
                TenantId = _currentUser.TenantId,
                OwnerType = (OwnerType)request.OwnerType,
                OwnerId = request.OwnerId,
                Purpose = (ConsentPurpose)request.Purpose,
                OptIn = request.OptIn,
                Channel = request.Channel,
                At = request.At ?? DateTime.UtcNow,
                Source = request.Source,
                Notes = request.Notes,
                ValidUntil = request.ValidUntil,
                Created = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId,
                LastModified = DateTime.UtcNow,
                LastModifiedBy = _currentUser.UserId
            };

            // Add domain event for consent creation
            consent.AddDomainEvent(new ConsentCreatedEvent(consent));

            _context.Consents.Add(consent);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Get owner display name for response
        string ownerDisplayName = await GetOwnerDisplayNameAsync(
            (OwnerType)request.OwnerType, request.OwnerId, cancellationToken);

        var isExpired = consent.ValidUntil.HasValue && consent.ValidUntil.Value < DateTime.UtcNow;
        var isActive = consent.OptIn && !isExpired;

        return new ConsentDto
        {
            Id = consent.Id,
            OwnerType = request.OwnerType,
            OwnerTypeName = ((OwnerType)request.OwnerType).ToString(),
            OwnerId = request.OwnerId,
            Purpose = request.Purpose,
            PurposeName = ((ConsentPurpose)request.Purpose).ToString(),
            OptIn = consent.OptIn,
            Channel = consent.Channel,
            At = consent.At,
            Source = consent.Source,
            Notes = consent.Notes,
            ValidUntil = consent.ValidUntil,
            Created = consent.Created,
            CreatedBy = consent.CreatedBy,
            LastModified = consent.LastModified,
            LastModifiedBy = consent.LastModifiedBy,
            IsActive = isActive,
            IsExpired = isExpired,
            StatusDescription = consent.OptIn
                ? (isExpired ? "Expired" : "Active")
                : "Withdrawn"
        };
    }

    private async Task<string> GetOwnerDisplayNameAsync(OwnerType ownerType, Guid ownerId, CancellationToken cancellationToken)
    {
        return ownerType switch
        {
            OwnerType.Client => await _context.Clients
                .Where(c => c.Id == ownerId.ToString())
                .Select(c => c.Name)
                .FirstOrDefaultAsync(cancellationToken) ?? "Unknown Client",

            OwnerType.Contact => await _context.Contacts
                .Where(c => c.Id == ownerId.ToString())
                .Select(c => $"{c.FirstName} {c.LastName}")
                .FirstOrDefaultAsync(cancellationToken) ?? "Unknown Contact",

            _ => "Unknown Owner"
        };
    }
}