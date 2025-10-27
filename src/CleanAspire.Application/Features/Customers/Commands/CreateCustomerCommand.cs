// Summary:
// This file defines a command and its handler for creating a new customer in the database. 
// The CreateCustomerCommand encapsulates the necessary data for a customer, while the 
// CreateCustomerCommandHandler processes the command, creates a customer entity, 
// and commits the changes. This ensures a structured and efficient approach to handling customer creation.

using CleanAspire.Application.Features.Customers.DTOs;
using CleanAspire.Application.Pipeline;

namespace CleanAspire.Application.Features.Customers.Commands;

// Command object that encapsulates the data required for creating a new customer. 
// Its fields directly map to the properties of CustomerDto.
public record CreateCustomerCommand(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Address
) : IFusionCacheRefreshRequest<CustomerDto>,
    IRequiresValidation
{
    public IEnumerable<string>? Tags => new[] { "customers" };
}

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerDto>
{
    private readonly IApplicationDbContext _context;

    public CreateCustomerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = new Customer
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(cancellationToken);

        return new CustomerDto() 
        { 
            Id = customer.Id, 
            FirstName = customer.FirstName, 
            LastName = customer.LastName,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            Address = customer.Address
        };
    }
}