using System.Globalization;
using CleanAspire.Application.Features.Customers.DTOs;
using CsvHelper;

namespace CleanAspire.Application.Features.Customers.Queries;

/// <summary>
/// Represents a query to export customers based on specified keywords.
/// </summary>
/// <param name="Keywords">The keywords to filter customers by FirstName, LastName, Email, or PhoneNumber.</param>
public record ExportCustomersQuery(string Keywords) : IRequest<Stream>;

/// <summary>
/// Handles the export of customers based on the provided query.
/// </summary>
public class ExportCustomersQueryHandler : IRequestHandler<ExportCustomersQuery, Stream>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExportCustomersQueryHandler"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public ExportCustomersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles the export customers query and returns a CSV stream of the filtered customers.
    /// </summary>
    /// <param name="request">The export customers query request.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A stream containing the CSV data of the filtered customers.</returns>
    public async ValueTask<Stream> Handle(ExportCustomersQuery request, CancellationToken cancellationToken)
    {
        var data = await _context.Customers
            .Where(x => x.FirstName.Contains(request.Keywords) 
                     || x.LastName.Contains(request.Keywords) 
                     || x.Email.Contains(request.Keywords) 
                     || x.PhoneNumber.Contains(request.Keywords))
            .Select(t => new CustomerDto
            {
                Id = t.Id,
                FirstName = t.FirstName,
                LastName = t.LastName,
                Email = t.Email,
                PhoneNumber = t.PhoneNumber,
                Address = t.Address
            }).ToListAsync(cancellationToken);

        var stream = new MemoryStream();
        using (var writer = new StreamWriter(stream, leaveOpen: true))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(data);
            await writer.FlushAsync();
        }
        return stream;
    }
}