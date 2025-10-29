using CleanAspire.Application.Features.Clients.DTOs;
using CleanAspire.Domain.Entities;

namespace CleanAspire.Application.Features.Clients.Queries;

/// <summary>
/// Query to fetch clients with pagination, filtering, and sorting options.
/// Implements IFusionCacheRequest to enable caching for performance optimization.
/// </summary>
public record ClientsWithPaginationQuery(
    string Keywords = "",
    int PageNumber = 0,
    int PageSize = 15,
    string OrderBy = "Name",
    string SortDirection = "Ascending",
    ClientType? FilterByType = null,
    ClientStatus? FilterByStatus = null,
    ClientPriority? FilterByPriority = null
) : IFusionCacheRequest<PaginatedResult<ClientDto>>
{
    /// <summary>
    /// Tags for cache invalidation, categorizing this query under "clients".
    /// </summary>
    public IEnumerable<string>? Tags => new[] { "clients" };

    /// <summary>
    /// Cache key for storing the result of this query, unique to its parameters.
    /// </summary>
    public string CacheKey => $"clientswithpagination_{Keywords}_{PageNumber}_{PageSize}_{OrderBy}_{SortDirection}_{FilterByType}_{FilterByStatus}_{FilterByPriority}";
}

/// <summary>
/// Handler for the ClientsWithPaginationQuery.
/// Retrieves paginated, filtered, and sorted client data from the database.
/// </summary>
public class ClientsWithPaginationQueryHandler : IRequestHandler<ClientsWithPaginationQuery, PaginatedResult<ClientDto>>
{
    private readonly IApplicationDbContext _context;

    public ClientsWithPaginationQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<PaginatedResult<ClientDto>> Handle(ClientsWithPaginationQuery request, CancellationToken cancellationToken)
    {
        // Build the base query with optional filters
        var query = _context.Clients.AsQueryable();

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

        // Apply priority filter
        if (request.FilterByPriority.HasValue)
        {
            query = query.Where(x => x.Priority == request.FilterByPriority.Value);
        }

        // Apply keyword search
        if (!string.IsNullOrEmpty(request.Keywords))
        {
            var keywords = request.Keywords.ToLower();
            query = query.Where(x => 
                x.Name.ToLower().Contains(keywords) ||
                (x.TradeName != null && x.TradeName.ToLower().Contains(keywords)) ||
                (x.Email != null && x.Email.ToLower().Contains(keywords)) ||
                (x.DocumentNumber != null && x.DocumentNumber.Contains(keywords)) ||
                (x.Industry != null && x.Industry.ToLower().Contains(keywords)) ||
                (x.Tags != null && x.Tags.ToLower().Contains(keywords))
            );
        }

        // Include contacts for counting
        query = query.Include(x => x.Contacts);

        // Retrieve and paginate data with mapping
        var data = await query
            .OrderBy(request.OrderBy, request.SortDirection)
            .ProjectToPaginatedDataAsync(
                condition: x => true, // Already filtered above
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                mapperFunc: c => new ClientDto
                {
                    Id = c.Id.ToString(),
                    Name = c.Name,
                    TradeName = c.TradeName,
                    DocumentNumber = c.DocumentNumber,
                    Email = c.Email,
                    Phone = c.Phone,
                    Website = c.Website,
                    Address = c.Address,
                    City = c.City,
                    State = c.State,
                    PostalCode = c.PostalCode,
                    Country = c.Country,
                    Industry = c.Industry,
                    Size = c.Size,
                    AnnualRevenue = c.AnnualRevenue,
                    EmployeeCount = c.EmployeeCount,
                    Type = (int)c.Type,
                    TypeName = c.Type.ToString(),
                    Status = (int)c.Status,
                    StatusName = c.Status.ToString(),
                    Priority = (int)c.Priority,
                    PriorityName = c.Priority.ToString(),
                    Notes = c.Notes,
                    Tags = c.Tags,
                    FirstContactDate = c.FirstContactDate,
                    LastInteractionDate = c.LastInteractionDate,
                    Created = c.Created,
                    CreatedBy = c.CreatedBy,
                    LastModified = c.LastModified,
                    LastModifiedBy = c.LastModifiedBy,
                    ContactsCount = c.Contacts.Count
                },
                cancellationToken: cancellationToken);

        return data;
    }
}