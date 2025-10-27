// Summary:
// This file defines a command and its handler for updating customer details in the database. 
// The UpdateCustomerCommand encapsulates the necessary data to update a customer, while the 
// UpdateCustomerCommandHandler validates the customer's existence, updates its details, 
// and commits the changes.

using CleanAspire.Application.Features.Customers.DTOs;
using CleanAspire.Application.Pipeline;

namespace CleanAspire.Application.Features.Customers.Commands;

// Command object that encapsulates the data required to update a customer. 
// Each field corresponds to a property in CustomerDto.
public record UpdateCustomerCommand(
    string Id,
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

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, CustomerDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateCustomerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<CustomerDto> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers.FindAsync(new object[] { request.Id }, cancellationToken);
        if (customer == null)
        {
            throw new KeyNotFoundException($"Customer with Id '{request.Id}' was not found.");
        }

        customer.FirstName = request.FirstName;
        customer.LastName = request.LastName;
        customer.Email = request.Email;
        customer.PhoneNumber = request.PhoneNumber;
        customer.Address = request.Address;

        _context.Customers.Update(customer);

        await _context.SaveChangesAsync(cancellationToken);

        return new CustomerDto
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