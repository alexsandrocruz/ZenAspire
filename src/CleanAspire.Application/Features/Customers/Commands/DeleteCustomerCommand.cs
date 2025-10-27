// Summary:
// This file defines a command and its handler for deleting customers from the database. 
// The DeleteCustomerCommand encapsulates the customer IDs to be deleted, while the 
// DeleteCustomerCommandHandler processes the command, removes the corresponding customers, 
// and commits the changes. This ensures a structured and efficient approach to handling customer deletions.

using CleanAspire.Application.Pipeline;

namespace CleanAspire.Application.Features.Customers.Commands;

// Command object that encapsulates the ID of the customer to be deleted.
public record DeleteCustomerCommand(string Id)
    : IFusionCacheRefreshRequest<bool>,
      IRequiresValidation
{
    public IEnumerable<string>? Tags => new[] { "customers" };
}

public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, bool>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteCustomerCommandHandler(ILogger<DeleteCustomerCommandHandler> logger, IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<bool> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer == null)
        {
            return false;
        }

        _dbContext.Customers.Remove(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}