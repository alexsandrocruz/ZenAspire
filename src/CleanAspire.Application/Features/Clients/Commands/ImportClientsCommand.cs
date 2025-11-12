using CleanAspire.Application.Pipeline;
using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Events;
using System.Globalization;
using System.Text;

namespace CleanAspire.Application.Features.Clients.Commands;

/// <summary>
/// Command for importing clients from a CSV file.
/// Processes a stream containing client data in CSV format.
/// </summary>
public record ImportClientsCommand(Stream Data) : IFusionCacheRefreshRequest<Unit>, IRequiresValidation
{
    public IEnumerable<string>? Tags => new[] { "clients" };
}

/// <summary>
/// Handler for processing ImportClientsCommand.
/// Parses CSV data and creates client entities in bulk.
/// </summary>
public class ImportClientsCommandHandler : IRequestHandler<ImportClientsCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public ImportClientsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Unit> Handle(ImportClientsCommand request, CancellationToken cancellationToken)
    {
        var clients = new List<Client>();
        
        using var reader = new StreamReader(request.Data, Encoding.UTF8);
        
        // Skip header line
        await reader.ReadLineAsync();
        
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            
            var values = ParseCsvLine(line);
            if (values.Length < 3) continue; // Need at least Name, Email, Type
            
            try
            {
                var client = new Client
                {
                    Name = values[0]?.Trim() ?? string.Empty,
                    Email = values[1]?.Trim(),
                    Type = ParseClientType(values[2]?.Trim()),
                    TradeName = values.Length > 3 ? values[3]?.Trim() : null,
                    DocumentNumber = values.Length > 4 ? values[4]?.Trim() : null,
                    Phone = values.Length > 5 ? values[5]?.Trim() : null,
                    Website = values.Length > 6 ? values[6]?.Trim() : null,
                    Address = values.Length > 7 ? values[7]?.Trim() : null,
                    City = values.Length > 8 ? values[8]?.Trim() : null,
                    State = values.Length > 9 ? values[9]?.Trim() : null,
                    PostalCode = values.Length > 10 ? values[10]?.Trim() : null,
                    Country = values.Length > 11 ? values[11]?.Trim() : null,
                    Industry = values.Length > 12 ? values[12]?.Trim() : null,
                    Size = values.Length > 13 ? values[13]?.Trim() : null,
                    AnnualRevenue = values.Length > 14 ? ParseDecimal(values[14]) : null,
                    EmployeeCount = values.Length > 15 ? ParseInt(values[15]) : null,
                    Status = values.Length > 16 ? ParseClientStatus(values[16]?.Trim()) : ClientStatus.Prospect,
                    Priority = values.Length > 17 ? ParseClientPriority(values[17]?.Trim()) : ClientPriority.Medium,
                    Notes = values.Length > 18 ? values[18]?.Trim() : null,
                    Tags = values.Length > 19 ? values[19]?.Trim() : null,
                    FirstContactDate = DateTime.UtcNow,
                    LastInteractionDate = DateTime.UtcNow
                };

                if (!string.IsNullOrEmpty(client.Name))
                {
                    client.AddDomainEvent(new ClientCreatedEvent(client));
                    clients.Add(client);
                }
            }
            catch (Exception)
            {
                // Skip invalid rows
                continue;
            }
        }

        if (clients.Any())
        {
            _context.Clients.AddRange(clients);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }

    private static string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        
        result.Add(current.ToString());
        return result.ToArray();
    }

    private static ClientType ParseClientType(string? value)
    {
        return value?.ToLowerInvariant() switch
        {
            "company" or "empresa" => ClientType.Company,
            "public figure" or "publicfigure" or "figura publica" => ClientType.PublicFigure,
            "government" or "governo" => ClientType.Government,
            "nonprofit" or "non-profit" or "ong" => ClientType.NonProfit,
            _ => ClientType.Company
        };
    }

    private static ClientStatus ParseClientStatus(string? value)
    {
        return value?.ToLowerInvariant() switch
        {
            "prospect" or "prospecto" => ClientStatus.Prospect,
            "active" or "ativo" => ClientStatus.Active,
            "inactive" or "inativo" => ClientStatus.Inactive,
            "churned" or "perdido" => ClientStatus.Churned,
            "blocked" or "bloqueado" => ClientStatus.Blocked,
            _ => ClientStatus.Prospect
        };
    }

    private static ClientPriority ParseClientPriority(string? value)
    {
        return value?.ToLowerInvariant() switch
        {
            "low" or "baixa" => ClientPriority.Low,
            "medium" or "media" => ClientPriority.Medium,
            "high" or "alta" => ClientPriority.High,
            "vip" => ClientPriority.VIP,
            _ => ClientPriority.Medium
        };
    }

    private static decimal? ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return decimal.TryParse(value, NumberStyles.Currency, CultureInfo.InvariantCulture, out var result) ? result : null;
    }

    private static int? ParseInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return int.TryParse(value, out var result) ? result : null;
    }
}