using CleanAspire.Application.Features.Contacts.DTOs;
using CleanAspire.Domain.Entities;

namespace CleanAspire.Application.Features.Contacts.Queries;

/// <summary>
/// Query to fetch all contacts without pagination.
/// Useful for dropdowns, exports, and simple lists.
/// </summary>
public record GetAllContactsQuery : IFusionCacheRequest<List<ContactDto>>
{
    /// <summary>
    /// Cache key for storing the result of this query.
    /// </summary>
    public string CacheKey => "all_contacts";

    /// <summary>
    /// Tags for cache invalidation, categorizing this query under "contacts".
    /// </summary>
    public IEnumerable<string>? Tags => new[] { "contacts" };
}

/// <summary>
/// Handler for the GetAllContactsQuery.
/// Retrieves all contacts from the database and maps them to DTOs.
/// </summary>
public class GetAllContactsQueryHandler : IRequestHandler<GetAllContactsQuery, List<ContactDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllContactsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<List<ContactDto>> Handle(GetAllContactsQuery request, CancellationToken cancellationToken)
    {
        var contacts = await _context.Contacts
            .Include(c => c.Client)
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
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