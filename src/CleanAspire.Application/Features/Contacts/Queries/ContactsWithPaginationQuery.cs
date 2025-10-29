using CleanAspire.Application.Features.Contacts.DTOs;
using CleanAspire.Domain.Entities;

namespace CleanAspire.Application.Features.Contacts.Queries;

/// <summary>
/// Query to fetch contacts with pagination, filtering, and sorting options.
/// Implements IFusionCacheRequest to enable caching for performance optimization.
/// </summary>
public record ContactsWithPaginationQuery(
    string Keywords = "",
    int PageNumber = 0,
    int PageSize = 15,
    string OrderBy = "FirstName",
    string SortDirection = "Ascending",
    string? FilterByClientId = null,
    ContactType? FilterByType = null,
    ContactStatus? FilterByStatus = null,
    bool? FilterByMainContact = null,
    bool? FilterByDecisionMaker = null
) : IFusionCacheRequest<PaginatedResult<ContactDto>>
{
    /// <summary>
    /// Tags for cache invalidation, categorizing this query under "contacts".
    /// </summary>
    public IEnumerable<string>? Tags => new[] { "contacts" };

    /// <summary>
    /// Cache key for storing the result of this query, unique to its parameters.
    /// </summary>
    public string CacheKey => $"contactswithpagination_{Keywords}_{PageNumber}_{PageSize}_{OrderBy}_{SortDirection}_{FilterByClientId}_{FilterByType}_{FilterByStatus}_{FilterByMainContact}_{FilterByDecisionMaker}";
}

/// <summary>
/// Handler for the ContactsWithPaginationQuery.
/// Retrieves paginated, filtered, and sorted contact data from the database.
/// </summary>
public class ContactsWithPaginationQueryHandler : IRequestHandler<ContactsWithPaginationQuery, PaginatedResult<ContactDto>>
{
    private readonly IApplicationDbContext _context;

    public ContactsWithPaginationQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<PaginatedResult<ContactDto>> Handle(ContactsWithPaginationQuery request, CancellationToken cancellationToken)
    {
        // Build the base query with optional filters
        var query = _context.Contacts.Include(c => c.Client).AsQueryable();

        // Apply client filter
        if (!string.IsNullOrEmpty(request.FilterByClientId))
        {
            query = query.Where(x => x.ClientId == request.FilterByClientId);
        }

        // Apply type filter
        if (request.FilterByType.HasValue)
        {
            query = query.Where(x => x.Type == request.FilterByType.Value);
        }

        // Apply status filter
        if (request.FilterByStatus.HasValue)
        {
            query = query.Where(x => x.Status == request.FilterByStatus.Value);
        }

        // Apply main contact filter
        if (request.FilterByMainContact.HasValue)
        {
            query = query.Where(x => x.IsMainContact == request.FilterByMainContact.Value);
        }

        // Apply decision maker filter
        if (request.FilterByDecisionMaker.HasValue)
        {
            query = query.Where(x => x.IsDecisionMaker == request.FilterByDecisionMaker.Value);
        }

        // Apply keyword search
        if (!string.IsNullOrEmpty(request.Keywords))
        {
            var keywords = request.Keywords.ToLower();
            query = query.Where(x => 
                x.FirstName.ToLower().Contains(keywords) ||
                x.LastName.ToLower().Contains(keywords) ||
                x.Email.ToLower().Contains(keywords) ||
                (x.Phone != null && x.Phone.Contains(keywords)) ||
                (x.MobilePhone != null && x.MobilePhone.Contains(keywords)) ||
                (x.JobTitle != null && x.JobTitle.ToLower().Contains(keywords)) ||
                (x.Department != null && x.Department.ToLower().Contains(keywords)) ||
                (x.Tags != null && x.Tags.ToLower().Contains(keywords)) ||
                x.Client.Name.ToLower().Contains(keywords)
            );
        }

        // Retrieve and paginate data with mapping
        var data = await query
            .OrderBy(request.OrderBy, request.SortDirection)
            .ProjectToPaginatedDataAsync(
                condition: x => true, // Already filtered above
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                mapperFunc: c => new ContactDto
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
                },
                cancellationToken: cancellationToken);

        return data;
    }
}