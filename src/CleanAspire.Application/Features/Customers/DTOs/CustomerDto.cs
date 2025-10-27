// Summary:
// This file defines a data transfer object (DTO) for customers.
// The CustomerDto class encapsulates customer details for data transfer between application layers.

namespace CleanAspire.Application.Features.Customers.DTOs;

// A DTO representing a customer, used to transfer data between application layers.
// Field names match the corresponding entity fields from the Customer domain entity.
public class CustomerDto
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    
    // Computed property for display purposes
    public string FullName => $"{FirstName} {LastName}".Trim();
}