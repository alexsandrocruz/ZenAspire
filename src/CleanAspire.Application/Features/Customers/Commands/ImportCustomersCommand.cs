// Summary:
// This file defines a command and its handler for importing customers from a CSV file.
// The ImportCustomersCommand encapsulates the input stream, while the ImportCustomersCommandHandler 
// reads the CSV data, maps it to customer entities, and commits the changes to the database.

using System.Globalization;
using CleanAspire.Application.Features.Customers.DTOs;
using CleanAspire.Application.Pipeline;
using CsvHelper;

namespace CleanAspire.Application.Features.Customers.Commands;

// Command object that encapsulates the input stream containing CSV data.
public record ImportCustomersCommand(Stream Stream)
    : IFusionCacheRefreshRequest<Unit>,
      IRequiresValidation
{
    public IEnumerable<string>? Tags => new[] { "customers" };
}

public class ImportCustomersCommandHandler : IRequestHandler<ImportCustomersCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public ImportCustomersCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Unit> Handle(ImportCustomersCommand request, CancellationToken cancellationToken)
    {
        request.Stream.Position = 0;

        using (var reader = new StreamReader(request.Stream))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var records = csv.GetRecords<CustomerDto>();

            foreach (var customer in records.Select(x => new Customer
            {
                FirstName = x.FirstName,
                LastName = x.LastName,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                Address = x.Address
            }))
            {
                _context.Customers.Add(customer);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}