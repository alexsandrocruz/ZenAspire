using CleanAspire.Application.Features.Customers.Commands;

namespace CleanAspire.Application.Features.Customers.Validators;

/// <summary>
/// Validator for DeleteCustomerCommand.
/// Uses FluentValidation to apply validation rules for customer deletion.
/// </summary>
public class DeleteCustomerCommandValidator : AbstractValidator<DeleteCustomerCommand>
{
    /// <summary>
    /// Initializes validation rules for customer deletion.
    /// </summary>
    public DeleteCustomerCommandValidator()
    {
        // Validate Id (must not be empty)
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Customer ID is required.");
    }
}