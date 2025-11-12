using CleanAspire.Application.Features.Clients.DTOs;
using CleanAspire.Domain.Entities;

namespace CleanAspire.Application.Features.Clients.Queries;

/// <summary>
/// Query to fetch a client by its ID.
/// Implements IFusionCacheRequest to enable caching.
/// </summary>
public record GetClientByIdQuery(string Id) : IFusionCacheRequest<ClientDto?>
{
    /// <summary>
    /// Cache key for storing the result of this query, specific to the client ID.
    /// </summary>
    public string CacheKey => $"client_{Id}";

    /// <summary>
    /// Tags for cache invalidation, categorizing this query under "clients".
    /// </summary>
    public IEnumerable<string>? Tags => new[] { "clients" };
}

/// <summary>
/// Handler for the GetClientByIdQuery.
/// Fetches a single ClientDto by its ID from the database.
/// </summary>
public class GetClientByIdQueryHandler : IRequestHandler<GetClientByIdQuery, ClientDto?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetClientByIdQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<ClientDto?> Handle(GetClientByIdQuery request, CancellationToken cancellationToken)
    {
        var client = await _dbContext.Clients
            .Include(c => c.Contacts)
            .Where(c => c.Id == request.Id)
            .Select(c => new ClientDto
            {
                Id = c.Id,
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
            })
            .SingleOrDefaultAsync(cancellationToken);

        return client;
    }
}