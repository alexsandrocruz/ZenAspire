using CleanAspire.Application.Features.Customers.DTOs;

namespace CleanAspire.Application.Features.Customers.Queries;

/// <summary>
/// Query to fetch customers with pagination, filtering, and sorting options.
/// Implements IFusionCacheRequest to enable caching for performance optimization.
/// </summary>
public record CustomersWithPaginationQuery(
    string Keywords,
    int PageNumber = 0,
    int PageSize = 15,
    string OrderBy = "Id",
    string SortDirection = "Descending"
) : IFusionCacheRequest<PaginatedResult<CustomerDto>>
{
    /// <summary>
    /// Tags for cache invalidation, categorizing this query under "customers".
    /// </summary>
    public IEnumerable<string>? Tags => new[] { "customers" };

    /// <summary>
    /// Cache key for storing the result of this query, unique to its parameters.
    /// </summary>
    public string CacheKey => $"customerswithpagination_{Keywords}_{PageNumber}_{PageSize}_{OrderBy}_{SortDirection}";
}

/// <summary>
/// Handler for the CustomersWithPaginationQuery.
/// Retrieves paginated, filtered, and sorted customer data from the database.
/// </summary>
public class CustomersWithPaginationQueryHandler : IRequestHandler<CustomersWithPaginationQuery, PaginatedResult<CustomerDto>>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Constructor to initialize the database context.
    /// </summary>
    /// <param name="context">Application database context.</param>
    public CustomersWithPaginationQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles the query by retrieving paginated customer data, applying filtering, sorting, and mapping to CustomerDto.
    /// </summary>
    /// <param name="request">The query request containing pagination, filtering, and sorting parameters.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A paginated result of CustomerDto objects.</returns>
    public async ValueTask<PaginatedResult<CustomerDto>> Handle(CustomersWithPaginationQuery request, CancellationToken cancellationToken)
    {
        // Retrieves and paginates data, applying filters and mapping to CustomerDto
        var data = await _context.Customers
                    .OrderBy(request.OrderBy, request.SortDirection) // Dynamic ordering
                    .ProjectToPaginatedDataAsync(
                        condition: x => x.FirstName.Contains(request.Keywords)
                                     || x.LastName.Contains(request.Keywords)
                                     || x.Email.Contains(request.Keywords)
                                     || x.PhoneNumber.Contains(request.Keywords), // Filter by keywords
                        pageNumber: request.PageNumber,
                        pageSize: request.PageSize,
                        mapperFunc: t => new CustomerDto // Map to CustomerDto
                        {
                            Id = t.Id,
                            FirstName = t.FirstName,
                            LastName = t.LastName,
                            Email = t.Email,
                            PhoneNumber = t.PhoneNumber,
                            Address = t.Address
                        },
                    cancellationToken: cancellationToken);

        return data;
    }
}