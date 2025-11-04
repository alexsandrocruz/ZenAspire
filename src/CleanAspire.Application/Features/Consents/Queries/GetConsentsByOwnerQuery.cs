using CleanAspire.Application.Features.Consents.DTOs;
using CleanAspire.Application.Pipeline;
using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Enums;

namespace CleanAspire.Application.Features.Consents.Queries;

/// <summary>
/// Query for retrieving consents by owner.
/// Phase 4: LGPD Compliance - Consent & Data Privacy
/// </summary>
public record GetConsentsByOwnerQuery : IFusionCacheRequest<List<ConsentListDto>>
{
    public int OwnerType { get; init; }
    public Guid OwnerId { get; init; }
    public bool IncludeInactive { get; init; } = false;

    public IEnumerable<string>? Tags => new[] { "consents", $"owner-{OwnerType}", $"owner-{OwnerId}" };
}

/// <summary>
/// Handler for processing GetConsentsByOwnerQuery.
/// Retrieves all consents for a specific owner with optional filtering.
/// </summary>
public class GetConsentsByOwnerQueryHandler : IRequestHandler<GetConsentsByOwnerQuery, List<ConsentListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetConsentsByOwnerQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<List<ConsentListDto>> Handle(GetConsentsByOwnerQuery request, CancellationToken cancellationToken)
    {
        // Validate tenant
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            throw new UnauthorizedAccessException("TenantId is required");

        // Validate enum values
        if (!Enum.IsDefined(typeof(OwnerType), request.OwnerType))
            throw new ArgumentException("Invalid OwnerType value");

        var query = _context.Consents
            .Where(c => c.TenantId == _currentUser.TenantId &&
                       c.OwnerType == (OwnerType)request.OwnerType &&
                       c.OwnerId == request.OwnerId);

        // Filter by active status if requested
        if (!request.IncludeInactive)
        {
            query = query.Where(c => c.OptIn &&
                                   (c.ValidUntil == null || c.ValidUntil > DateTime.UtcNow));
        }

        var consents = await query
            .OrderByDescending(c => c.At)
            .ToListAsync(cancellationToken);

        // Get owner display name
        string ownerDisplayName = await GetOwnerDisplayNameAsync(
            (OwnerType)request.OwnerType, request.OwnerId, cancellationToken);

        return consents.Select(c => new ConsentListDto
        {
            Id = c.Id,
            OwnerTypeName = c.OwnerType.ToString(),
            OwnerDisplayName = ownerDisplayName,
            PurposeName = c.Purpose.ToString(),
            OptIn = c.OptIn,
            Channel = c.Channel,
            At = c.At,
            ValidUntil = c.ValidUntil,
            IsActive = c.IsActive,
            StatusDescription = c.OptIn
                ? (c.ValidUntil.HasValue && c.ValidUntil.Value < DateTime.UtcNow ? "Expired" : "Active")
                : "Withdrawn"
        }).ToList();
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

/// <summary>
/// Query for retrieving a single consent by ID.
/// </summary>
public record GetConsentByIdQuery : IFusionCacheRequest<ConsentDto?>
{
    public string Id { get; init; } = string.Empty;

    public IEnumerable<string>? Tags => new[] { "consents" };
}

/// <summary>
/// Handler for processing GetConsentByIdQuery.
/// Retrieves a specific consent by its ID.
/// </summary>
public class GetConsentByIdQueryHandler : IRequestHandler<GetConsentByIdQuery, ConsentDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetConsentByIdQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<ConsentDto?> Handle(GetConsentByIdQuery request, CancellationToken cancellationToken)
    {
        // Validate tenant
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            throw new UnauthorizedAccessException("TenantId is required");

        var consent = await _context.Consents
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.TenantId == _currentUser.TenantId,
                cancellationToken);

        if (consent == null)
            return null;

        // Get owner display name
        string ownerDisplayName = await GetOwnerDisplayNameAsync(
            consent.OwnerType, consent.OwnerId, cancellationToken);

        return new ConsentDto
        {
            Id = consent.Id,
            OwnerType = (int)consent.OwnerType,
            OwnerTypeName = consent.OwnerType.ToString(),
            OwnerId = consent.OwnerId,
            Purpose = (int)consent.Purpose,
            PurposeName = consent.Purpose.ToString(),
            OptIn = consent.OptIn,
            Channel = consent.Channel,
            At = consent.At,
            Source = consent.Source,
            IpAddress = consent.IpAddress,
            UserAgent = consent.UserAgent,
            Notes = consent.Notes,
            ValidUntil = consent.ValidUntil,
            Created = consent.Created,
            CreatedBy = consent.CreatedBy,
            LastModified = consent.LastModified,
            LastModifiedBy = consent.LastModifiedBy,
            IsActive = consent.IsActive,
            IsExpired = consent.ValidUntil.HasValue && consent.ValidUntil.Value < DateTime.UtcNow,
            StatusDescription = consent.OptIn
                ? (consent.ValidUntil.HasValue && consent.ValidUntil.Value < DateTime.UtcNow ? "Expired" : "Active")
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