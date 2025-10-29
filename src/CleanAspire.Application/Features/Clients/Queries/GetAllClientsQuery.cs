using CleanAspire.Application.Features.Clients.DTOs;
using CleanAspire.Domain.Entities;

namespace CleanAspire.Application.Features.Clients.Queries;

/// <summary>
/// Query to fetch all clients without pagination.
/// Useful for dropdowns, exports, and simple lists.
/// </summary>
public record GetAllClientsQuery : IFusionCacheRequest<List<ClientDto>>
{
    /// <summary>
    /// Cache key for storing the result of this query.
    /// </summary>
    public string CacheKey => "all_clients";

    /// <summary>
    /// Tags for cache invalidation, categorizing this query under "clients".
    /// </summary>
    public IEnumerable<string>? Tags => new[] { "clients" };
}

/// <summary>
/// Handler for the GetAllClientsQuery.
/// Retrieves all clients from the database and maps them to DTOs.
/// </summary>
public class GetAllClientsQueryHandler : IRequestHandler<GetAllClientsQuery, List<ClientDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllClientsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<List<ClientDto>> Handle(GetAllClientsQuery request, CancellationToken cancellationToken)
    {
        var clients = await _context.Clients
            .Include(c => c.Contacts)
            .OrderBy(c => c.Name)
            .Select(c => new ClientDto
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
            })
            .ToListAsync(cancellationToken);

        return clients;
    }
}