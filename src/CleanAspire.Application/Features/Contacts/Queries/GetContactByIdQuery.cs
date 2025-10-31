using CleanAspire.Application.Features.Contacts.DTOs;

namespace CleanAspire.Application.Features.Contacts.Queries;

/// <summary>
/// Query to fetch a contact by its ID.
/// Implements IFusionCacheRequest to enable caching.
/// </summary>
public record GetContactByIdQuery(string Id) : IFusionCacheRequest<ContactDto?>
{
    /// <summary>
    /// Cache key for storing the result of this query, specific to the contact ID.
    /// </summary>
    public string CacheKey => $"contact_{Id}";

    /// <summary>
    /// Tags for cache invalidation, categorizing this query under "contacts".
    /// </summary>
    public IEnumerable<string>? Tags => new[] { "contacts" };
}

/// <summary>
/// Handler for the GetContactByIdQuery.
/// Fetches a single ContactDto by its ID from the database.
/// </summary>
public class GetContactByIdQueryHandler : IRequestHandler<GetContactByIdQuery, ContactDto?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetContactByIdQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<ContactDto?> Handle(GetContactByIdQuery request, CancellationToken cancellationToken)
    {
        var contact = await _dbContext.Contacts
            .Include(c => c.Client)
            .Where(c => c.Id == request.Id)
            .Select(c => new ContactDto
            {
                Id = c.Id,
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
                ClientId = c.ClientId,
                ClientName = c.Client.Name,
                ClientDisplayName = c.Client.DisplayName,
                Created = c.Created,
                CreatedBy = c.CreatedBy,
                LastModified = c.LastModified,
                LastModifiedBy = c.LastModifiedBy
            })
            .SingleOrDefaultAsync(cancellationToken);

        return contact;
    }
}