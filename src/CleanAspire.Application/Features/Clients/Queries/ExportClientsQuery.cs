using CleanAspire.Domain.Entities;
using System.Text;

namespace CleanAspire.Application.Features.Clients.Queries;

/// <summary>
/// Query to export clients data to CSV format.
/// Implements IFusionCacheRequest to enable caching of export data.
/// </summary>
public record ExportClientsQuery(
    string Keywords = "",
    ClientType? FilterByType = null,
    ClientStatus? FilterByStatus = null,
    ClientPriority? FilterByPriority = null
) : IFusionCacheRequest<byte[]>
{
    /// <summary>
    /// Cache key for storing the result of this query.
    /// </summary>
    public string CacheKey => $"export_clients_{Keywords}_{FilterByType}_{FilterByStatus}_{FilterByPriority}";

    /// <summary>
    /// Tags for cache invalidation, categorizing this query under "clients".
    /// </summary>
    public IEnumerable<string>? Tags => new[] { "clients" };
}

/// <summary>
/// Handler for the ExportClientsQuery.
/// Exports client data to CSV format.
/// </summary>
public class ExportClientsQueryHandler : IRequestHandler<ExportClientsQuery, byte[]>
{
    private readonly IApplicationDbContext _context;

    public ExportClientsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<byte[]> Handle(ExportClientsQuery request, CancellationToken cancellationToken)
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

        var clients = await query
            .Include(c => c.Contacts)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        var csv = new StringBuilder();
        
        // Add CSV header
        csv.AppendLine("Name,TradeName,DocumentNumber,Email,Phone,Website,Address,City,State,PostalCode,Country,Industry,Size,AnnualRevenue,EmployeeCount,Type,Status,Priority,Notes,Tags,FirstContactDate,LastInteractionDate,ContactsCount,Created,CreatedBy");

        // Add data rows
        foreach (var client in clients)
        {
            csv.AppendLine($"\"{EscapeCsvValue(client.Name)}\"," +
                          $"\"{EscapeCsvValue(client.TradeName)}\"," +
                          $"\"{EscapeCsvValue(client.DocumentNumber)}\"," +
                          $"\"{EscapeCsvValue(client.Email)}\"," +
                          $"\"{EscapeCsvValue(client.Phone)}\"," +
                          $"\"{EscapeCsvValue(client.Website)}\"," +
                          $"\"{EscapeCsvValue(client.Address)}\"," +
                          $"\"{EscapeCsvValue(client.City)}\"," +
                          $"\"{EscapeCsvValue(client.State)}\"," +
                          $"\"{EscapeCsvValue(client.PostalCode)}\"," +
                          $"\"{EscapeCsvValue(client.Country)}\"," +
                          $"\"{EscapeCsvValue(client.Industry)}\"," +
                          $"\"{EscapeCsvValue(client.Size)}\"," +
                          $"{client.AnnualRevenue?.ToString("F2") ?? ""}," +
                          $"{client.EmployeeCount?.ToString() ?? ""}," +
                          $"\"{client.Type}\"," +
                          $"\"{client.Status}\"," +
                          $"\"{client.Priority}\"," +
                          $"\"{EscapeCsvValue(client.Notes)}\"," +
                          $"\"{EscapeCsvValue(client.Tags)}\"," +
                          $"{client.FirstContactDate?.ToString("yyyy-MM-dd") ?? ""}," +
                          $"{client.LastInteractionDate?.ToString("yyyy-MM-dd") ?? ""}," +
                          $"{client.Contacts.Count}," +
                          $"{client.Created?.ToString("yyyy-MM-dd HH:mm:ss") ?? ""}," +
                          $"\"{EscapeCsvValue(client.CreatedBy)}\"");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    private static string EscapeCsvValue(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // Escape quotes by doubling them
        return value.Replace("\"", "\"\"");
    }
}