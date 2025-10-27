// Summary:
// This file defines a query and its handler for retrieving all customers.
// The GetAllCustomersQuery returns all customers from the database mapped to DTOs.

using CleanAspire.Application.Features.Customers.DTOs;

namespace CleanAspire.Application.Features.Customers.Queries;

public record GetAllCustomersQuery : IRequest<IList<CustomerDto>>;

public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, IList<CustomerDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllCustomersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<IList<CustomerDto>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        var customers = await _context.Customers
            .OrderBy(x => x.FirstName)
            .ToListAsync(cancellationToken);

        return customers.Select(x => new CustomerDto
        {
            Id = x.Id,
            FirstName = x.FirstName,
            LastName = x.LastName,
            Email = x.Email,
            PhoneNumber = x.PhoneNumber,
            Address = x.Address
        }).ToList();
    }
}