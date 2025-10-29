using CleanAspire.Application.Features.Contacts.DTOs;

namespace CleanAspire.Application.Features.Contacts.Queries;

/// <summary>
/// Query to fetch all contacts for a specific client.
/// Useful for client detail pages and related contact lists.
/// </summary>
public record GetContactsByClientIdQuery(string ClientId) : IFusionCacheRequest<List<ContactDto>>
{
    /// <summary>
    /// Cache key for storing the result of this query.
    /// </summary>
    public string CacheKey => $"contacts_by_client_{ClientId}";

    /// <summary>
    /// Tags for cache invalidation, categorizing this query under "contacts".
    /// </summary>
    public IEnumerable<string>? Tags => new[] { "contacts", "clients" };
}

/// <summary>
/// Handler for the GetContactsByClientIdQuery.
/// Retrieves all contacts for a specific client from the database and maps them to DTOs.
/// </summary>
public class GetContactsByClientIdQueryHandler : IRequestHandler<GetContactsByClientIdQuery, List<ContactDto>>
{
    private readonly IApplicationDbContext _context;

    public GetContactsByClientIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<List<ContactDto>> Handle(GetContactsByClientIdQuery request, CancellationToken cancellationToken)
    {
        var contacts = await _context.Contacts
            .Include(c => c.Client)
            .Where(c => c.ClientId == request.ClientId)
            .OrderBy(c => c.IsMainContact ? 0 : 1) // Main contacts first
            .ThenBy(c => c.FirstName)
            .ThenBy(c => c.LastName)
            .Select(c => new ContactDto
            {
                Id = c.Id.ToString(),
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                Phone = c.Phone,
                MobilePhone = c.MobilePhone,
                JobTitle = c.JobTitle,
                Department = c.Department,
                Address = c.Address,
                City = c.City,
                State = c.State,
                PostalCode = c.PostalCode,
                Notes = c.Notes,
                Tags = c.Tags,
                Type = (int)c.Type,
                TypeName = c.Type.ToString(),
                Status = (int)c.Status,
                StatusName = c.Status.ToString(),
                IsMainContact = c.IsMainContact,
                IsDecisionMaker = c.IsDecisionMaker,
                BirthDate = c.BirthDate,
                LastContactDate = c.LastContactDate,
                ClientId = c.ClientId.ToString(),
                ClientName = c.Client.Name,
                ClientDisplayName = c.Client.DisplayName,
                Created = c.Created,
                CreatedBy = c.CreatedBy,
                LastModified = c.LastModified,
                LastModifiedBy = c.LastModifiedBy
            })
            .ToListAsync(cancellationToken);

        return contacts;
    }
}